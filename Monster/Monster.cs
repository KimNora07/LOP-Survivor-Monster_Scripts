using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Mirror;
using Lop.Survivor.inventroy;

public class Monster : NetworkBehaviour, IDamageable
{
    #region 변수
    // 몬스터 상태 관련
    public MonsterState currentState;

    // 몬스터들의 데이터가 저장된 ScriptableObject
    public MonsterDataScriptable monsterData;

    // 몬스터 데이터 관련
    [Header("Status Variables")]
    public string id;                                           // 몬스터 id(이름)
    public string nameID;                    // 몬스터 이름 id
    [SyncVar] public float hp;                          // 몬스터 체력
    public float moveSpeed;                  // 몬스터 이동속도
    public float atkPower;                   // 몬스터 공격력
    public float atkInterval;                // 몬스터 공격 주기
    public float detectRange;                // 몬스터 감지 범위
    public float atkRange;                   // 몬스터 공격 범위
    [HideInInspector] public EDetectType detectType;           // 몬스터 감지 상태
    public bool isAttack;                    // 몬스터의 액션 상태(true: 공격, false: 도망)
    public CurrenctData[] currenctDatas;      // 드랍 아이템 데이터 배열
    public float[] value;

    public bool isAttacking;

    // 몬스터의 타겟 감지 관련
    public List<PenguinBody> penguins;  // 월드에 생성된 펭귄들의 리스트
    public Transform target;        // 처음으로 감지된 펭귄을 지정

    // 행동 관련
    public float attackTimer;            // 현재 남은 공격 쿨타임
    [HideInInspector] public float rotationSpeed = 5f;     // 회전 속도

    // 컴포넌트 관련
    protected Rigidbody rb;
    [HideInInspector] public Animator animator;

    // 순찰 관련 
    protected float changeDirectionTimeMin = 5f;
    protected float changeDirectionTimeMax = 10f;
    protected Vector3 movedirection;
    public float changeDirectionTime;
    protected bool isGoOutFromSpawnRange;
    [HideInInspector] public bool isTargetBuildingEnter = false;
    
    // 스폰 관련
    public Vector3 startPosition;   // 처음으로 스폰된 위치
    public MonsterSpawner spawner;  // 자신을 스폰한 스포너 컴포넌트

    // Bool
    [HideInInspector] public bool isDead                = false;    // 죽었는지 판단
    [HideInInspector] public bool isTargetInSight       = false;    // 공격 대기중 일 때 타겟을 보고 있는지 판단
    [HideInInspector] public bool isAttacked            = false;    // 공격을 받았는지 판단
    [HideInInspector] public bool isPatrolling                            = false;    // 순찰 중인지 판단 
    
    // 오브젝트가 파괴될 때 실행시켜주는 이벤트
    public delegate void MonsterDestroyedHandler();
    public event MonsterDestroyedHandler OnDestroyed;

    // 오브젝트가 파괴되기 전 애니메이션이 모두 실행되게 함
    private WaitForSeconds waitingDieAnimation = new WaitForSeconds(2.5f);

    public int lineSize;    

    protected Lop.Survivor.Island.Effect.EffectManager inGameEffectManager;
    private Inventory inventory;
    #endregion

    private void Start()
    {
        animator = GetComponent<Animator>();
        penguins = FindObjectsOfType<PenguinBody>().ToList();
        rb = GetComponent<Rigidbody>();
        inGameEffectManager = FindAnyObjectByType<Lop.Survivor.Island.Effect.EffectManager>();
        inventory = FindAnyObjectByType<PenguinFunction>().inventory;

        MonsterDataLoader.Load(this, this.monsterData);

        ChangeState(new IdleState(this));

        if (!isServer) { return; }
    }
    
    private void Update()
    {
        if (!isServer) { return; }
        if (hp <= 0) ChangeState(new DieState(this));

        IsTargetInsideBuilding(this.target);

        attackTimer -= Time.deltaTime;
        if (attackTimer <= 0) isAttacking = false;  // 쿨타임이 지나면 다시 공격 가능

        currentState?.Update();

        if (Input.GetKeyDown(KeyCode.P))
        {
            Debug.Log(id + ": " + currentState);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!isServer) { return; }

        // 스포너에서 몬스터가 소환됬을 때 스포너 변수에 MonsterSpawner클래스 컴포넌트가 있는 오브젝트를 반환
        if (other.CompareTag(MonsterTag.Spawner))
        {
            spawner = other.GetComponent<MonsterSpawner>();
            startPosition = other.transform.position;
        }

        if (other.CompareTag(MonsterTag.AttackCollider))
        {
            IsAttackble attackDamagble = other.gameObject.GetComponent<IsAttackble>();
            DealDamage(attackDamagble.Damaged);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (!isServer) { return; }

        if (other.CompareTag(MonsterTag.Spawner))
        {
            isGoOutFromSpawnRange = true;
        }
    }

    private void OnDrawGizmos()
    {
        // Gizmo 색상 설정
        Gizmos.color = Color.red;

        // 반원 형태로 시각화
        Vector3 forwardDirection = transform.forward;

        Vector3 leftBoundary = Quaternion.Euler(0, -45 * 0.5f, 0) * forwardDirection;
        Vector3 rightBoundary = Quaternion.Euler(0, 45 * 0.5f, 0) * forwardDirection;

        Gizmos.DrawLine(transform.position, transform.position + leftBoundary * atkRange);
        Gizmos.DrawLine(transform.position, transform.position + rightBoundary * atkRange);
        Gizmos.DrawWireSphere(transform.position, atkRange);
    }

    private void OnDestroy()
    {
        if (!isServer) { return; }

        // OnDestroyed인 이벤트에 구독한 메소드가 있는지 검사 후 실행
        OnDestroyed?.Invoke();
    }

    #region 상태 메서드 오버라이딩
    public virtual void Idle() { }
    public virtual void Action() { }
    public virtual void WaitForAttack() { }
    public virtual void Attack() { }
    public virtual void Die() { }
    #endregion

    public void Patrol()
    {
        if (!isPatrolling) return;

        animator.SetBool(MonsterAnimationName.isRun, true);

        // 쿨타임이 다된 후 랜덤으로 방향을 바꿈
        changeDirectionTime -= Time.deltaTime;
        if (changeDirectionTime <= 0)
        {
            ChangeDirection();
        }

        // 회전 후 해당 방향으로 이동(회전 후 forward 방향으로 이동)
        // 목표 회전 계산
        if (movedirection != Vector3.zero && target != null)
        {
            Quaternion targetRotation = Quaternion.LookRotation(movedirection);

            if(Quaternion.Angle(transform.rotation, targetRotation) > 0.1f)
            {
                rb.MoveRotation(Quaternion.Slerp(rb.rotation, targetRotation, Time.deltaTime * rotationSpeed));
            }
        }
        Vector3 moveStep = transform.forward * moveSpeed * Time.deltaTime;
        rb.MovePosition(transform.position + moveStep);
    }

    /// <summary>
    /// 스포너 범위 내로 돌아오는 메서드
    /// </summary>
    public void ReturnToStart()
    {
        animator.SetBool(MonsterAnimationName.isRun, true);

        Vector3 directionToStart = new Vector3(spawner.transform.position.x, transform.position.y, spawner.transform.position.z) - transform.position;
        directionToStart = directionToStart.normalized;

        Quaternion targetRotation = Quaternion.LookRotation(directionToStart);
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * rotationSpeed);

        // 회전 후 해당 방향으로 이동
        Vector3 moveStep = transform.forward * moveSpeed * Time.deltaTime;
        rb.MovePosition(transform.position + moveStep);

        if (Vector3.Distance(transform.position, spawner.transform.position) < spawner.rangeX * 0.5f)
        {
            isGoOutFromSpawnRange = false;
            changeDirectionTime = 0;
        }
    }

    public void AttackPlayer()
    {
        if (isTargetBuildingEnter)
        {
            ChangeState(new IdleState(this));
            target = null;
            return;
        }

        // 플레이어 hp -= atk_power;
        Collider[] targetsInRange = Physics.OverlapSphere(transform.position, atkRange);

        foreach (Collider target in targetsInRange)
        {
            if (target.CompareTag(MonsterTag.Player) && !IsTargetInAttackRange(target.transform))
            {
                isTargetInSight = false;
                ChangeState(new WaitForAttackState(this));
                return;
            }

            // 각도와 거리 확인
            if (target.CompareTag(MonsterTag.Player) && IsTargetInAttackRange(target.transform))
            {
                Debug.Log("공격 범위안에 들어와서 공격!");
                isTargetInSight = true;
                RpcSetTrigger(MonsterAnimationName.isAttack);
                attackTimer = atkInterval;
            }
        }
    }

    protected virtual void AttackSound() { }   

    /// <summary>
    /// 몬스터의 움직이는 방향을 랜덤으로 지정하는 메서드
    /// </summary>
    public void ChangeDirection()
    {
        float angle = Random.Range(0f, 360f);
        movedirection = new Vector3(Mathf.Cos(angle), 0, Mathf.Sin(angle)).normalized;

        changeDirectionTime = Random.Range(changeDirectionTimeMin, changeDirectionTimeMax);
    }

    /// <summary>
    /// 타겟이 공격범위에 들어와 있는지 확인하는 메서드
    /// </summary>
    /// <param name="target"></param>
    /// <returns></returns>
    public bool IsTargetInAttackRange(Transform target)
    {
        Vector3 directionToTarget = (target.position - transform.position).normalized;
        float distanceToTarget = Vector3.Distance(transform.position, target.position);

        // 거리와 각도 확인
        if (distanceToTarget <= atkRange)
        {
            float angleToTarget = Vector3.Angle(transform.forward, directionToTarget);
            if (angleToTarget <= 45)
            {
                return true;
            }
        }
        return false;
    }

    /// <summary>
    /// 공격 받았을 때 실행하는 메서드(애니메이션 이벤트 트리거 사용)
    /// </summary>
    /// <param name="atk">공격한 상대의 공격력</param>
    public void GetHit(float atk)
    {
        hp -= atk;
        RpcSetTrigger(MonsterAnimationName.isGetHit);
    }
    public virtual void DealDamage(float atk)
    {
        if (hp <= 0) return;

        hp -= atk;
        inGameEffectManager.PlayEffect("EF_Crash_01", string.Empty, this.transform.position + new Vector3(0, 0.5f, 0.25f));
        RpcSetTrigger(MonsterAnimationName.isGetHit);
    }

    /// <summary>
    /// 타겟이 건물안에 들어간지를 확인하는 메서드
    /// </summary>
    private void IsTargetInsideBuilding(Transform target)
    {
        if (target != null) isTargetBuildingEnter = (target.GetComponent<PenguinBody>().isBuildingEnter) ? true : false;

        if (isTargetBuildingEnter)
        {
            ChangeState(new IdleState(this));
        }
    }

    #region 애니메이션 이벤트 트리거와 연동된 메서드
    /// <summary>
    /// 몬스터가 공격하고 피해를 입히는 것을 애니메이션 이벤트와 연동시킴
    /// </summary>
    public void OnAttackHit()
    {
        AttackSound();

        // 타겟이 범위 내에 있고 공격이 아직 유효할 때만 피해를 적용합니다.
        if (target != null && IsTargetInAttackRange(target) && isServer)
        {
            IDamageable dealDamage = target.GetComponent<IDamageable>();
            if (dealDamage != null)
            {
                dealDamage.DealDamage(atkPower);
            }
        }
    }
    #endregion

    #region 서버 관련 메서드
    [ClientRpc]
    public void RpcSetTrigger(string trigger)
    {
        animator.SetTrigger(trigger);
    }
    #endregion

    /// <summary>
    /// 가장 가까운 타겟을 찾는 메서드
    /// </summary>
    /// <returns></returns>
    public Transform FindClosetTarget()
    {
        penguins.RemoveAll(p => p == null);
        return penguins
            .Where(p => Vector3.Distance(this.transform.position, p.transform.position) <= detectRange)
            .OrderBy(p => Vector3.Distance(transform.position, p.transform.position))
            .FirstOrDefault()?.transform;
    }

    /// <summary>
    /// 새로운 타겟을 지정하는 메서드
    /// </summary>
    /// <param name="newTarget"></param>
    public void SetTarget(Transform newTarget) => this.target = newTarget;

    /// <summary>
    /// 스포너 범위 밖으로 나갔는지 확인하는 메서드
    /// </summary>
    /// <returns></returns>
    public bool IsOutOfSpawnRange() => Vector3.Distance(transform.position, startPosition) > detectRange;

    /// <summary>
    /// 상태 변경 메서드
    /// </summary>
    /// <param name="newState"></param>
    public void ChangeState(MonsterState newState)
    {
        currentState?.Exit();
        currentState = newState;
        currentState?.Enter();
    }

    public IEnumerator DropItem(CurrenctData type, int amount)
    {
        yield return waitingDieAnimation;
        for (int i = 0; i < amount; i++)
        {
            inventory.DropItemGameObject(type.currenctType.ToString(), new Vector3(this.transform.position.x, this.transform.position.y + 0.5f, this.transform.position.z));
        }
    }

    /// <summary>
    /// 죽었을 때 애니메이션 실행 후 제거 하는 코루틴
    /// </summary>
    /// <returns></returns>
    public IEnumerator DestroyObject()
    {
        yield return waitingDieAnimation;

        NetworkObjectManager.Instance.DestroyNetworkObject(this.gameObject);
    }

    /// <summary>
    /// 공격했을 때 애니메이션 실행 후 false로 반환 하는 코루틴 
    /// </summary>
    /// <returns></returns>
    public IEnumerator Co_Attacked()
    {
        yield return new WaitForSeconds(1f);
        isAttacked = false;
    }

    public void Jump()
    {
        RaycastHit hit;
        if (Physics.Raycast(transform.TransformPoint(new Vector3(0, 1f, 0)), transform.forward, out hit, lineSize))
        {
            if (hit.collider.gameObject.CompareTag(MonsterTag.Ground))
            {
                Vector3 jumpVelocity = rb.velocity;
                jumpVelocity.y = 5f; // 점프 속도 설정 (이 값을 조정하여 점프 높이를 조절)
                rb.velocity = jumpVelocity;
            }
        }
    }

    #region Action State
    /// <summary>
    /// 타겟을 따라가는 메서드
    /// </summary>
    public void FollowToTarget()
    {
        if (target == null) return;

        Vector3 directionToStart = new Vector3(target.position.x, transform.position.y, target.position.z) - transform.position;
        directionToStart = directionToStart.normalized;

        Quaternion targetRotation = Quaternion.LookRotation(directionToStart);
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * rotationSpeed);

        // 플레이어 방향으로 이동
        Vector3 dir = (target.position - transform.position).normalized;
        transform.position += dir * moveSpeed * Time.deltaTime;
        animator.SetBool(MonsterAnimationName.isRun, true);

        Jump();
    }

    /// <summary>
    /// 반대 방향으로 타겟에서 멀리 떨어지는 메서드
    /// </summary>
    public void AvoidToTarget()
    {
        Vector3 oppositeDirection = -transform.forward;
        oppositeDirection = oppositeDirection.normalized;

        Quaternion targetRotation = Quaternion.LookRotation(oppositeDirection);
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * rotationSpeed);

        Vector3 forwardMovement = transform.forward * moveSpeed * Time.deltaTime;
        transform.position += forwardMovement;

        Jump();
    }
    #endregion
}
