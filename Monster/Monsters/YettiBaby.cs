/// <summary>
/// 먼저 공격 받으면 동망감 / 감지 사거리 내에 몬스터가 공격 받으면 도망감
/// n일차 이후부터는 사냥 개시 후 스폰된 그룹 내의 모든 아기 예티를 처치하지 못하면,
/// 일정 시간 뒤에, 플레이어와 가장 가까운 위치의 아기 예티 위치에 엄마 예티 소환(value[0])
/// </summary>
public class YettiBaby : Monster
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
        base.WaitForAttack();

        if (attackTimer <= 0)
        {
            isAttacking = false; 
            ChangeState(new AttackState(this));
            return;
        }
    }
    public override void Attack()
    {
        base.Attack();
        if (attackTimer > 0) return;

        AttackPlayer();
        isAttacking = true;
        attackTimer = atkInterval;
    }
    public override void Die()
    {
        base.Die();
    }
    public override void DealDamage(float atk)
    {
        base.DealDamage(atk);
    }
}
