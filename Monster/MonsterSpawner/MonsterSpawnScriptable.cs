using UnityEngine;

[CreateAssetMenu(fileName = "New Monster Spawn Data", menuName = "Data/Monster/SpawnData")]
public class MonsterSpawnScriptable : ScriptableObject
{
    public MonsterSpawnData[] monsterSpawnDatas;
}
