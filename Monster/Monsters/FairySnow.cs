using UnityEngine;

/// <summary>
/// 감지 거리 내의 다른 몬스터가 공격 받으면 추격
/// 공격 사거리 내에 들어오면 공격
/// </summary>
public class FairySnow : Monster
{
    public override void Idle()
    {
        //Vector3 fixedPositionCenter = transform.TransformPoint(new Vector3(0, 1f, 0)); // ������ ��ġ
        //Vector3 fixedPositionLeft = transform.TransformPoint(new Vector3(-0.25f, 1f, 0.5f)); // ������ ��ġ
        //Vector3 fixedPositionRight = transform.TransformPoint(new Vector3(0.25f, 1f, 0.5f)); // ������ ��ġ

        //Debug.DrawRay(fixedPositionCenter, transform.forward * lineSize, Color.yellow);
        //Debug.DrawRay(fixedPositionLeft, transform.forward * lineSize, Color.yellow);
        //Debug.DrawRay(fixedPositionRight, transform.forward * lineSize, Color.yellow);

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
        SoundManager.Instance.PlaySFX("SnowFairyAttack");
    }
}
