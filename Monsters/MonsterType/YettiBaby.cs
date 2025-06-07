/// <summary>
/// ���� ���� ������ ������ / ���� ��Ÿ� ���� ���Ͱ� ���� ������ ������
/// n���� ���ĺ��ʹ� ��� ���� �� ������ �׷� ���� ��� �Ʊ� ��Ƽ�� óġ���� ���ϸ�,
/// ���� �ð� �ڿ�, �÷��̾�� ���� ����� ��ġ�� �Ʊ� ��Ƽ ��ġ�� ���� ��Ƽ ��ȯ(value[0])
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
