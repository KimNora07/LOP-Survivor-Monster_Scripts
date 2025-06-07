/// <summary>
/// 감지 거리 내에 들어오면 항상 추격
/// 공격 사거리 내에 들어오면 공격
/// 공격 준비 상태에서 이동 정지
/// 플레이어 방향으로 일정 거리만큼 돌진
/// </summary>
public class WildBoarDefault : Monster
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
        base.DealDamage(atk);
    }
    protected override void AttackSound()
    {
        SoundManager.Instance.PlaySFX("AngryBoarAttack");
    }
}
