using System.Collections.Generic;
using UnityEngine;

using Mirror;

using Lop.Survivor;

public class MonsterSpawner : NetworkBehaviour
{
    public MonsterSpawnScriptable monsterSpawnDatas;

    public string groupID;                  // 몬스터 생성 그룹 id
    public int conditionDay;                // 그룹을 활성화할 날짜, 해당 날짜 이후부터 생성이 가능해짐
    public TimeType conditionTimeType;      // 몬스터를 스폰(또는 재생성)할 시점
    public float groupInterval;             // 그룹을 재생성할 쿨타임
    public int monsterGroupCount;           // 생성할 몬스터 그룹의 수량

    public MonsterGroup[] monsterGroups;
    public Dictionary<string, GameObject> monsterPrefabDict;

    public int rangeX;  // 그룹 범위의 X
    public int rangeZ;  // 그룹 범위의 Y

    public List<Vector3> spawnedPositions = new List<Vector3>();    // 이미 배치된 몬스터 위치 저장 리스트
    public float minDistance = 2.0f;                                // 몬스터 간 최소 거리

    private bool canSpawn = false;

    private MonsterList monsterList;

    private void Start()
    {   
        if (!isServer) { return; }
        
        MonsterSpawnerDataLoad();

        monsterList = FindAnyObjectByType<MonsterList>();

        // Dictionary 초기화
        monsterPrefabDict = new Dictionary<string, GameObject>();

        // 배열의 길이가 같다는 가정 하에, 배열을 순회하면서 Dictionary에 값을 추가
        for (int i = 0; i < monsterGroups.Length; i++)
        {
            // ID 배열과 프리팹 배열의 동일 인덱스를 사용하여 Dictionary에 추가
            if (i < monsterGroups.Length && monsterGroups[i].monsterPrefab != null)
            {
                monsterPrefabDict.Add(monsterGroups[i].monsterID, monsterGroups[i].monsterPrefab);
            }
            else
            {
                Debug.LogError($"몬스터 id에 알맞는 오브젝트인 '{monsterGroups[i]}'를 찾지 못했습니다!");
            }
        }

        SetColliderRange();

        InvokeRepeating(nameof(SpawnStart), 0, 1);
    }

    #region 몬스터 스포너 데이터 불러오기
    /// <summary>
    /// 몬스터 스포너의 데이터를 불어오는 메소드
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

                // monsterGroup 배열의 깊은 복사 수행
                monsterGroups = new MonsterGroup[data.monsterGroups.Length];
                for (int i = 0; i < data.monsterGroups.Length; i++)
                {
                    // 깊은 복사 생성자를 사용하여 각 요소를 독립적으로 복사
                    monsterGroups[i] = new MonsterGroup(data.monsterGroups[i]);
                }
                break;
            }
        }
    }
    #endregion

    #region 몬스터 스폰
    /// <summary>
    /// 몬스터를 스폰하게 하는 메소드
    /// </summary>
    /// <param name="monster_id">몬스터의 id(string)</param>
    private void SpawnMonster(string monster_id)
    {
        // Dictionary에서 monster_id에 해당하는 프리팹을 찾음
        if (monsterPrefabDict.TryGetValue(monster_id, out GameObject prefab))
        {
            // 해당 monster_id를 가진 MonsterGroup을 찾음
            MonsterGroup group = System.Array.Find(monsterGroups, mg => mg.monsterID == monster_id);

            // 활성화된 몬스터가 그룹의 최대 소환 수보다 적으면 소환
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

                    // 몬스터가 파괴될 때 활성화된 몬스터 수 감소 처리
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

                    // 몬스터가 파괴될 때 활성화된 몬스터 수 감소 처리
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
            Debug.LogError($"'{monster_id}'인 몬스터 id를 Dictionary에서 찾지 못했습니다");
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
                    //Debug.Log($"{monster.monster_id}가 스폰 되었습니다!");
                }
            }
        }
        else
        {
            //Debug.Log($"현재 시간: {TimeManager.Instance.CurrentTimeType} / 현재 일 수: {TimeManager.Instance.CurrentDay}");
            //Debug.Log("현제 시간 또는 일수가 소환 조건에 알맞지 않습니다!");
        }
    }

    /// <summary>
    /// 박스콜라이더 컴포넌트가 없을 경우 넣어주고, 박스콜라이더의 크기를 지정
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
    /// 박스콜라이더 범위 내에서 스폰 될 위치를 랜덤 지정
    /// </summary>
    /// <returns></returns>
    private Vector3 GetRandomPositionInCollider()
    {
        BoxCollider boxCollider = GetComponent<BoxCollider>();

        float randomX = Random.Range(-boxCollider.size.x / 2, boxCollider.size.x / 2);
        float randomZ = Random.Range(-boxCollider.size.z / 2, boxCollider.size.z / 2);

        Vector3 spawnPosition = transform.position + new Vector3(randomX, 0, randomZ);

        // 지형 높이 확인
        if (Physics.Raycast(spawnPosition + Vector3.up * 5, Vector3.down, out RaycastHit hitInfo, 20f))
        {
            spawnPosition.y = hitInfo.point.y + 1.5f;  // 지형의 높이로 Y값 설정
            Debug.Log("지형의 높이로 Y값을 설정");
        }
        else
        {
            spawnPosition.y = transform.position.y + 1.5f;  // 기본 높이로 설정
            Debug.Log("기본 높이로 설정");
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
            // Penguin.cs를 가진 오브젝트가 있을 경우에만 몬스터를 스폰하게 한다(플레이어가 존재했을 경우)
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
