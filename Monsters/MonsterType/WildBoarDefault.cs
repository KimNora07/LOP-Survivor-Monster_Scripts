/// <summary>
/// ���� �Ÿ� ���� ������ �׻� �߰�
/// ���� ��Ÿ� ���� ������ ����
/// ���� �غ� ���¿��� �̵� ����
/// �÷��̾� �������� ���� �Ÿ���ŭ ����
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
