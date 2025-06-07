using System.Collections.Generic;
using UnityEngine;

using Mirror;

using Lop.Survivor;

public class MonsterSpawner : NetworkBehaviour
{
    public MonsterSpawnScriptable monsterSpawnDatas;

    public string groupID;                  // ���� ���� �׷� id
    public int conditionDay;                // �׷��� Ȱ��ȭ�� ��¥, �ش� ��¥ ���ĺ��� ������ ��������
    public TimeType conditionTimeType;      // ���͸� ����(�Ǵ� �����)�� ����
    public float groupInterval;             // �׷��� ������� ��Ÿ��
    public int monsterGroupCount;           // ������ ���� �׷��� ����

    public MonsterGroup[] monsterGroups;
    public Dictionary<string, GameObject> monsterPrefabDict;

    public int rangeX;  // �׷� ������ X
    public int rangeZ;  // �׷� ������ Y

    public List<Vector3> spawnedPositions = new List<Vector3>();    // �̹� ��ġ�� ���� ��ġ ���� ����Ʈ
    public float minDistance = 2.0f;                                // ���� �� �ּ� �Ÿ�

    private bool canSpawn = false;

    private MonsterList monsterList;

    private void Start()
    {   
        if (!isServer) { return; }
        
        MonsterSpawnerDataLoad();

        monsterList = FindAnyObjectByType<MonsterList>();

        // Dictionary �ʱ�ȭ
        monsterPrefabDict = new Dictionary<string, GameObject>();

        // �迭�� ���̰� ���ٴ� ���� �Ͽ�, �迭�� ��ȸ�ϸ鼭 Dictionary�� ���� �߰�
        for (int i = 0; i < monsterGroups.Length; i++)
        {
            // ID �迭�� ������ �迭�� ���� �ε����� ����Ͽ� Dictionary�� �߰�
            if (i < monsterGroups.Length && monsterGroups[i].monsterPrefab != null)
            {
                monsterPrefabDict.Add(monsterGroups[i].monsterID, monsterGroups[i].monsterPrefab);
            }
            else
            {
                Debug.LogError($"���� id�� �˸´� ������Ʈ�� '{monsterGroups[i]}'�� ã�� ���߽��ϴ�!");
            }
        }

        SetColliderRange();

        InvokeRepeating(nameof(SpawnStart), 0, 1);
    }

    #region ���� ������ ������ �ҷ�����
    /// <summary>
    /// ���� �������� �����͸� �Ҿ���� �޼ҵ�
    /// </summary>
    private void MonsterSpawnerDataLoad()
    {
        foreach (var data in monsterSpawnDatas.monsterSpawnDatas)
        {
            if (data.groupID == groupID)
            {
                conditionDay = data.conditionDay;
                conditionTimeType = data.conditionTimeType;
                groupInterval = data.groupInterval;
                monsterGroupCount = data.monsterGroupCount;

                // monsterGroup �迭�� ���� ���� ����
                monsterGroups = new MonsterGroup[data.monsterGroups.Length];
                for (int i = 0; i < data.monsterGroups.Length; i++)
                {
                    // ���� ���� �����ڸ� ����Ͽ� �� ��Ҹ� ���������� ����
                    monsterGroups[i] = new MonsterGroup(data.monsterGroups[i]);
                }
                break;
            }
        }
    }
    #endregion

    #region ���� ����
    /// <summary>
    /// ���͸� �����ϰ� �ϴ� �޼ҵ�
    /// </summary>
    /// <param name="monster_id">������ id(string)</param>
    private void SpawnMonster(string monster_id)
    {
        // Dictionary���� monster_id�� �ش��ϴ� �������� ã��
        if (monsterPrefabDict.TryGetValue(monster_id, out GameObject prefab))
        {
            // �ش� monster_id�� ���� MonsterGroup�� ã��
            MonsterGroup group = System.Array.Find(monsterGroups, mg => mg.monsterID == monster_id);

            // Ȱ��ȭ�� ���Ͱ� �׷��� �ִ� ��ȯ ������ ������ ��ȯ
            if (group.currentActiveMonsterCount < group.monsterAmount * monsterGroupCount)
            {
                Vector3 spawnPosition = GetRandomPositionInCollider();

                if (IsPositionValid(spawnPosition))
                {
                    GameObject go = Instantiate(prefab, spawnPosition, Quaternion.identity);
                    go.GetComponent<Monster>().startPosition = this.transform.position;
                    spawnedPositions.Add(spawnPosition);
                    group.currentActiveMonsterCount++;

                    monsterList.monsters.Add(go.GetComponent<Monster>());

                    // ���Ͱ� �ı��� �� Ȱ��ȭ�� ���� �� ���� ó��
                    go.GetComponent<Monster>().OnDestroyed += () =>
                    {
                        group.currentActiveMonsterCount--;
                        spawnedPositions.Remove(spawnPosition);

                        monsterList.monsters.Remove(go.GetComponent<Monster>());
                    };

                    NetworkServer.Spawn(go);
                }
                else
                {
                    Vector3 spawnPosition2 = GetRandomPositionInCollider();

                    GameObject go = Instantiate(prefab, spawnPosition2, Quaternion.identity);
                    go.GetComponent<Monster>().startPosition = this.transform.position;
                    spawnedPositions.Add(spawnPosition2);
                    group.currentActiveMonsterCount++;

                    monsterList.monsters.Add(go.GetComponent<Monster>());

                    // ���Ͱ� �ı��� �� Ȱ��ȭ�� ���� �� ���� ó��
                    go.GetComponent<Monster>().OnDestroyed += () =>
                    {
                        group.currentActiveMonsterCount--;
                        spawnedPositions.Remove(spawnPosition2);

                        monsterList.monsters.Remove(go.GetComponent<Monster>());
                    };

                    NetworkServer.Spawn(go);
                }
            }
        }
        else
        {
            Debug.LogError($"'{monster_id}'�� ���� id�� Dictionary���� ã�� ���߽��ϴ�");
        }
    }

    private void Spawn()
    {
        if ((TimeManager.Instance.CurrentTimeType == conditionTimeType || conditionTimeType == TimeType.All) && TimeManager.Instance.CurrentDay >= conditionDay)
        {
            foreach (var monster in monsterGroups)
            {
                for (int i = 0; i < monster.monsterAmount; i++)
                {
                    SpawnMonster(monster.monsterID);
                    //Debug.Log($"{monster.monster_id}�� ���� �Ǿ����ϴ�!");
                }
            }
        }
        else
        {
            //Debug.Log($"���� �ð�: {TimeManager.Instance.CurrentTimeType} / ���� �� ��: {TimeManager.Instance.CurrentDay}");
            //Debug.Log("���� �ð� �Ǵ� �ϼ��� ��ȯ ���ǿ� �˸��� �ʽ��ϴ�!");
        }
    }

    /// <summary>
    /// �ڽ��ݶ��̴� ������Ʈ�� ���� ��� �־��ְ�, �ڽ��ݶ��̴��� ũ�⸦ ����
    /// </summary>
    private void SetColliderRange()
    {
        BoxCollider boxCollider = GetComponent<BoxCollider>();

        if (boxCollider == null)
        {
            boxCollider = gameObject.AddComponent<BoxCollider>();
        }
        boxCollider.isTrigger = true;
        boxCollider.size = new Vector3(rangeX, 10, rangeZ);
        boxCollider.center = Vector3.zero;
    }

    /// <summary>
    /// �ڽ��ݶ��̴� ���� ������ ���� �� ��ġ�� ���� ����
    /// </summary>
    /// <returns></returns>
    private Vector3 GetRandomPositionInCollider()
    {
        BoxCollider boxCollider = GetComponent<BoxCollider>();

        float randomX = Random.Range(-boxCollider.size.x / 2, boxCollider.size.x / 2);
        float randomZ = Random.Range(-boxCollider.size.z / 2, boxCollider.size.z / 2);

        Vector3 spawnPosition = transform.position + new Vector3(randomX, 0, randomZ);

        // ���� ���� Ȯ��
        if (Physics.Raycast(spawnPosition + Vector3.up * 5, Vector3.down, out RaycastHit hitInfo, 20f))
        {
            spawnPosition.y = hitInfo.point.y + 1.5f;  // ������ ���̷� Y�� ����
            Debug.Log("������ ���̷� Y���� ����");
        }
        else
        {
            spawnPosition.y = transform.position.y + 1.5f;  // �⺻ ���̷� ����
            Debug.Log("�⺻ ���̷� ����");
        }

        return spawnPosition;
    }

    private bool IsPositionValid(Vector3 newPosition)
    {
        foreach (Vector3 pos in spawnedPositions)
        {
            if (Vector3.Distance(newPosition, pos) < minDistance)
            {
                return false;
            }
        }
        return true;
    }
    #endregion

    private void SpawnStart()
    {
        if (!canSpawn)
        {
            // Penguin.cs�� ���� ������Ʈ�� ���� ��쿡�� ���͸� �����ϰ� �Ѵ�(�÷��̾ �������� ���)
            if (FindObjectOfType<PenguinBody>())
            {
                canSpawn = true;
                InvokeRepeating(nameof(Spawn), 3, groupInterval);
            }
        }
    }

    public override void OnStopServer()
    {
        base.OnStopServer();

        foreach(var monster in monsterGroups)
        {
            monster.currentActiveMonsterCount = 0;
        }
    }
}
