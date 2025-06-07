using UnityEngine;

[System.Serializable]
public class MonsterGroup
{
    public string monsterID;                // 몬스터 id 정보
    public int monsterAmount;               // 몬스터 수량 정보
    public GameObject monsterPrefab;        // 소환할 몬스터 프리팹
    public int currentActiveMonsterCount;   // 현재 활성화된 몬스터의 수

    // 깊은 복사 생성자
    public MonsterGroup(MonsterGroup source)
    {
        // 깊은 복사 시 초기화 (각 스포너마다 독립적)
        monsterID = source.monsterID;
        monsterPrefab = source.monsterPrefab;
        monsterAmount = source.monsterAmount;
        currentActiveMonsterCount = 0; 
    }
}
