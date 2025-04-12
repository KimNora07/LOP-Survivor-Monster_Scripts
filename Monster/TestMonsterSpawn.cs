using UnityEngine;

public class TestMonsterSpawn : MonoBehaviour
{
    public static TestMonsterSpawn instance;
    public TimeType timeType;
    public int day;

    private void Awake()
    {
        instance = this;
    }
}
