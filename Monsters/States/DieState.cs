public class DieState : MonsterState
{
    public DieState(Monster monster) : base(monster) { }

    public override void Enter()
    {
        if (!monster.isDead)
        {
            monster.isDead = true;
            monster.animator.ResetTrigger(MonsterAnimationName.isGetHit);
            monster.animator.ResetTrigger(MonsterAnimationName.isAttack);
            monster.RpcSetTrigger(MonsterAnimationName.isDeath);
            foreach (var currenct in monster.currenctDatas)
            {
                monster.StartCoroutine(monster.DropItem(currenct, currenct.dropAmount));
            }
            monster.StartCoroutine(monster.DestroyObject());
        }

        monster.Die();
    }

    public override void Update()
    {

    }

    public override void Exit()
    {

    }
}
