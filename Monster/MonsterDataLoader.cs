using System.Linq;

public class MonsterDataLoader
{
    /// <summary>
    /// 몬스터 데이터를 가져오는 메서드
    /// </summary>
    public static void Load(Monster monster, MonsterDataScriptable monsterData)
    {
        if (monsterData == null) return;

        var data = monsterData.monsterDatas.FirstOrDefault(d => d.id == monster.id);
        if (data != null)
        {
            monster.nameID = data.nameID;
            /*if (isServer)*/ monster.hp = data.hp;
            monster.moveSpeed = data.moveSpeed;
            monster.atkPower = data.atkPower;
            monster.atkInterval = data.atkInterval;
            monster.detectRange = data.detectRange;
            monster.atkRange = data.atkRange;
            monster.detectType = data.detectType;
            monster.isAttack = data.isAttack;

            monster.currenctDatas = data.currenctDatas.ToArray();
            monster.value = data.value.ToArray();
        }
    }
}
