/// <summary>
/// ���� �Ÿ� ���� ������ �߰�
/// ���� ��Ÿ� ���� ������ ����
/// �÷��̾�� �Ÿ��� �����ٸ� �������� 1 or 2
/// �÷��̾ ���� ��Ÿ��� ���ݺ��� �ۿ� �ִٸ� �÷��̾� �������� �����̸� ����
/// 
/// ��������1
/// - ���� �� �̵� ����
/// - ������ ���� ������ ����
/// 
/// ��������2
/// - ���� �غ� ���¿��� �̵� ����
/// - �÷��̾� �������� ���� �Ÿ���ŭ ����(value[0])
/// 
/// ������ ������ ����
/// - ���� �÷��̾ ���ִ� �������� �����̸� ����
/// - ������ �ӵ��� ����(value[1])
/// - ������ �������� ����
/// - �̸� ��� ���������� Ȯ�� ����
/// 
/// Ư�� �׼�(�Ʊ⿹Ƽ�� ���� �����Ǿ��� ���)
/// ���� ��Ÿ� ���� �÷��̾ �ִٸ�, �̵� �ӵ��� value[2]��ŭ ����
/// ��) 1.5 -> 150%
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
