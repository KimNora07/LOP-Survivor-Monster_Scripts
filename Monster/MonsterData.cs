/// <summary>
/// 몬스터 데이터
/// </summary>
[System.Serializable]
public class MonsterData
{
    public string id;
    public string nameID;
    public float hp;
    public float moveSpeed;
    public float atkPower;
    public float atkInterval;
    public float detectRange;
    public float atkRange;
    public EDetectType detectType;
    public bool isAttack;
    public CurrenctData[] currenctDatas;
    public float[] value;
}
