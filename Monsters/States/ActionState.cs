using UnityEngine;

public class ActionState : MonsterState
{
    public ActionState(Monster monster) : base(monster) { }

    public override void Enter()
    {
        monster.isAttacked = false;
    }

    public override void Update()
    {
        if (monster.target == null)
        {
            monster.ChangeState(new IdleState(this.monster));
            return;
        }

        // ���� �������� ����� ��� ��� ���·� ��ȯ �� ����
        if (Vector3.Distance(monster.transform.position, monster.target.position) > monster.detectRange || monster.isTargetBuildingEnter)
        {
            monster.ChangeState(new IdleState(monster));
            monster.target = null;
            return;
        }

        // ���� ������ ������ ��� ���� ��� ���·� ��ȯ �� ����
        if (Vector3.Distance(monster.transform.position, monster.target.position) <= monster.atkRange)
        {
            monster.ChangeState(new WaitForAttackState(monster));
            return;
        }

        monster.Action();
    }

    public override void Exit()
    {

    }
}
