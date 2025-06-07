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

        // 감지 범위에서 벗어났을 경우 대기 상태로 전환 후 리턴
        if (Vector3.Distance(monster.transform.position, monster.target.position) > monster.detectRange || monster.isTargetBuildingEnter)
        {
            monster.ChangeState(new IdleState(monster));
            monster.target = null;
            return;
        }

        // 공격 범위에 들어왔을 경우 공격 대기 상태로 전환 후 리턴
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
