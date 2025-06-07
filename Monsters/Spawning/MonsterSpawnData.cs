[System.Serializable]
public class MonsterSpawnData
{
    public string groupID;                  // ���� ���� �׷� id
    public int conditionDay;                // �׷��� Ȱ��ȭ�� ��¥, �ش� ��¥ ���ĺ��� ������ ��������
    public TimeType conditionTimeType;      // ���͸� ����(�Ǵ� �����)�� ����
    public float groupInterval;             // �׷��� ������� ��Ÿ��
    public int monsterGroupCount;           // ������ ���� �׷��� ����
    public MonsterGroup[] monsterGroups;
}
