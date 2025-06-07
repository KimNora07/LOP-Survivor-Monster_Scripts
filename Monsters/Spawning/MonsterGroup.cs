using UnityEngine;

[System.Serializable]
public class MonsterGroup
{
    public string monsterID;                // ���� id ����
    public int monsterAmount;               // ���� ���� ����
    public GameObject monsterPrefab;        // ��ȯ�� ���� ������
    public int currentActiveMonsterCount;   // ���� Ȱ��ȭ�� ������ ��

    // ���� ���� ������
    public MonsterGroup(MonsterGroup source)
    {
        // ���� ���� �� �ʱ�ȭ (�� �����ʸ��� ������)
        monsterID = source.monsterID;
        monsterPrefab = source.monsterPrefab;
        monsterAmount = source.monsterAmount;
        currentActiveMonsterCount = 0; 
    }
}
