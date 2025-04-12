using UnityEngine;

/// <summary>
/// 감지 거리 내에 들어오면 항상 액션
/// </summary>
public class Mommoth : Monster
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
        if (hp <= 0) return;

        hp -= atk;
        inGameEffectManager.PlayEffect("EF_Crash_01", string.Empty, this.transform.position + new Vector3(0, 0.5f, 1.5f));
        RpcSetTrigger(MonsterAnimationName.isGetHit);
    }
    protected override void AttackSound()
    {
        SoundManager.Instance.PlaySFX("MommothAttack");
    }
}
