/// <summary>
/// 먼저 공격 받으면 도망
/// 이후, 감지 사거리 내에 들어오면 도망
/// 공격 안함
/// </summary>
public class TribeRabbit : Monster
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
        if (target == null) return;

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
