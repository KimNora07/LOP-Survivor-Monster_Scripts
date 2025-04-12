/// <summary>
/// 감지 거리 내에 들어오면 추격
/// 공격 사거리 내에 들어오면 공격
/// 플레이어와 거리가 가깝다면 랜덤패턴 1 or 2
/// 플레이어가 공격 사거리의 절반보다 밖에 있다면 플레이어 방향으로 눈덩이를 던짐
/// 
/// 랜덤패턴1
/// - 공격 시 이동 정지
/// - 전방의 범위 내에서 공격
/// 
/// 랜덤패턴2
/// - 공격 준비 상태에서 이동 정지
/// - 플레이어 방향으로 일정 거리만큼 돌진(value[0])
/// 
/// 눈덩이 던지는 패턴
/// - 현재 플레이어가 서있는 방향으로 눈덩이를 던짐
/// - 눈동이 속도는 빠름(value[1])
/// - 포물선 방향으로 던짐
/// - 미리 어디에 떨어지는지 확인 가능
/// 
/// 특수 액션(아기예티를 통해 생성되었을 경우)
/// 감지 사거리 내에 플레이어가 있다면, 이동 속도가 value[2]만큼 가속
/// 예) 1.5 -> 150%
/// </summary>
public class YettiMommy : Monster
{
    public override void Idle()
    {
        if (isPatrolling || isGoOutFromSpawnRange)
        {
            Jump();
        }
    }
    public override void Action()
    {
        if (isAttack)
        {
            FollowToTarget();
        }
        else
        {
            AvoidToTarget();
        }
    }
    public override void WaitForAttack()
    {
        if (attackTimer <= 0)
        {
            isAttacking = false;
            ChangeState(new AttackState(this));
            return;
        }
    }
    public override void Attack()
    {
        if (attackTimer > 0) return;

        AttackPlayer();
        isAttacking = true;
        attackTimer = atkInterval;
    }
    public override void Die() { }
    public override void DealDamage(float atk)
    {

    }
}
