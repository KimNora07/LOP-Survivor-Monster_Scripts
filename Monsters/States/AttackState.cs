public class AttackState : MonsterState
{
    public AttackState(Monster monster) : base(monster) { }

    public override void Enter()
    {

    }

    public override void Update()
    {
        if (monster.isTargetBuildingEnter)
        {
            monster.ChangeState(new IdleState(monster));
            monster.target = null;
            return;
        }

        // ���� ��Ÿ���� ���� �ʾ��� ��� ���� ��� ���·� ��ȯ �� ���� 
        if (monster.attackTimer > 0)
        {
            monster.ChangeState(new WaitForAttackState(monster));
            return;
        }

        if (monster.isAttacked)
        {
            monster.StartCoroutine(monster.Co_Attacked());
        }

        monster.Attack();
    }

    public override void Exit()
    {

    }
}
