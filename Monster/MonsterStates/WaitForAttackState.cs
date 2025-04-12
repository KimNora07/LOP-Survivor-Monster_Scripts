using UnityEngine;

public class WaitForAttackState : MonsterState
{
    public WaitForAttackState(Monster monster) : base(monster) { }
    
    public override void Enter()
    {
        monster.isAttacked = false;
    }

    public override void Update()
    {
        if (monster.target == null)
        {
            monster.ChangeState(new IdleState(monster));
            return;
        }

        // 타겟이 건물안에 들어갔을 경우
        if (monster.isTargetBuildingEnter)
        {
            monster.ChangeState(new IdleState(monster));
            monster.target = null;
            return;
        }

        if (Vector3.Distance(monster.transform.position, monster.target.position) > monster.atkRange)
        {
            monster.ChangeState(new ActionState(monster));
            return;
        }

        if (!monster.isTargetInSight)
        {
            // 서서히 플레이어를 바라보게 함
            Vector3 directionToStart = new Vector3(monster.target.position.x, monster.transform.position.y, monster.target.position.z) - monster.transform.position;
            directionToStart = directionToStart.normalized;

            Quaternion targetRotation = Quaternion.LookRotation(directionToStart);
            monster.transform.rotation = Quaternion.Slerp(monster.transform.rotation, targetRotation, Time.deltaTime * monster.rotationSpeed);
        }

        monster.WaitForAttack();
    }

    public override void Exit()
    {

    }
}
