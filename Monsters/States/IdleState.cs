public class IdleState : MonsterState
{
    public IdleState(Monster monster) : base(monster) { }

    public override void Enter()
    {
        monster.isAttacked = false;
        monster.isPatrolling = true;
    }

    public override void Update()
    {
        var closetTarget = monster.FindClosetTarget();
        if (closetTarget != null)
        {
            monster.SetTarget(closetTarget);
            monster.ChangeState(new ActionState(monster));
            return;
        }
        else
        {
            if (monster.IsOutOfSpawnRange()) monster.ReturnToStart();
            else
            {
                monster.Patrol();
            }
        }

        monster.Idle();
    }

    public override void Exit()
    {
        monster.isPatrolling = false;
    }
}
