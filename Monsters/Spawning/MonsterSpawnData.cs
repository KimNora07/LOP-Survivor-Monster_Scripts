[System.Serializable]
public class MonsterSpawnData
{
    public string groupID;                  // 몬스터 생성 그룹 id
    public int conditionDay;                // 그룹을 활성화할 날짜, 해당 날짜 이후부터 생성이 가능해짐
    public TimeType conditionTimeType;      // 몬스터를 스폰(또는 재생성)할 시점
    public float groupInterval;             // 그룹을 재생성할 쿨타임
    public int monsterGroupCount;           // 생성할 몬스터 그룹의 수량
    public MonsterGroup[] monsterGroups;
}
