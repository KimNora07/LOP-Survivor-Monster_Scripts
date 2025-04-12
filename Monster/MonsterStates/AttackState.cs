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

        // 공격 쿨타임이 돌지 않았을 경우 공격 대기 상태로 전환 후 리턴 
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
