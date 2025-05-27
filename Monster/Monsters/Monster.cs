using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Mirror;
using Lop.Survivor.inventroy;

public class Monster : NetworkBehaviour, IDamageable
{
    #region ����
    // ���� ���� ����
    public MonsterState currentState;

    // ���͵��� �����Ͱ� ����� ScriptableObject
    public MonsterDataScriptable monsterData;

    // ���� ������ ����
    [Header("Status Variables")]
    public string id;                                           // ���� id(�̸�)
    public string nameID;                    // ���� �̸� id
    [SyncVar] public float hp;                          // ���� ü��
    public float moveSpeed;                  // ���� �̵��ӵ�
    public float atkPower;                   // ���� ���ݷ�
    public float atkInterval;                // ���� ���� �ֱ�
    public float detectRange;                // ���� ���� ����
    public float atkRange;                   // ���� ���� ����
    [HideInInspector] public EDetectType detectType;           // ���� ���� ����
    public bool isAttack;                    // ������ �׼� ����(true: ����, false: ����)
    public CurrenctData[] currenctDatas;      // ��� ������ ������ �迭
    public float[] value;

    public bool isAttacking;

    // ������ Ÿ�� ���� ����
    public List<PenguinBody> penguins;  // ���忡 ������ ��ϵ��� ����Ʈ
    public Transform target;        // ó������ ������ ����� ����

    // �ൿ ����
    public float attackTimer;            // ���� ���� ���� ��Ÿ��
    [HideInInspector] public float rotationSpeed = 5f;     // ȸ�� �ӵ�

    // ������Ʈ ����
    protected Rigidbody rb;
    [HideInInspector] public Animator animator;

    // ���� ���� 
    protected float changeDirectionTimeMin = 5f;
    protected float changeDirectionTimeMax = 10f;
    protected Vector3 movedirection;
    public float changeDirectionTime;
    protected bool isGoOutFromSpawnRange;
    [HideInInspector] public bool isTargetBuildingEnter = false;
    
    // ���� ����
    public Vector3 startPosition;   // ó������ ������ ��ġ
    public MonsterSpawner spawner;  // �ڽ��� ������ ������ ������Ʈ

    // Bool
    [HideInInspector] public bool isDead                = false;    // �׾����� �Ǵ�
    [HideInInspector] public bool isTargetInSight       = false;    // ���� ����� �� �� Ÿ���� ���� �ִ��� �Ǵ�
    [HideInInspector] public bool isAttacked            = false;    // ������ �޾Ҵ��� �Ǵ�
    [HideInInspector] public bool isPatrolling                            = false;    // ���� ������ �Ǵ� 
    
    // ������Ʈ�� �ı��� �� ��������ִ� �̺�Ʈ
    public delegate void MonsterDestroyedHandler();
    public event MonsterDestroyedHandler OnDestroyed;

    // ������Ʈ�� �ı��Ǳ� �� �ִϸ��̼��� ��� ����ǰ� ��
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
        if (attackTimer <= 0) isAttacking = false;  // ��Ÿ���� ������ �ٽ� ���� ����

        currentState?.Update();

        if (Input.GetKeyDown(KeyCode.P))
        {
            Debug.Log(id + ": " + currentState);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!isServer) { return; }

        // �����ʿ��� ���Ͱ� ��ȯ���� �� ������ ������ MonsterSpawnerŬ���� ������Ʈ�� �ִ� ������Ʈ�� ��ȯ
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
        // Gizmo ���� ����
        Gizmos.color = Color.red;

        // �ݿ� ���·� �ð�ȭ
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

        // OnDestroyed�� �̺�Ʈ�� ������ �޼ҵ尡 �ִ��� �˻� �� ����
        OnDestroyed?.Invoke();
    }

    #region ���� �޼��� �������̵�
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

        // ��Ÿ���� �ٵ� �� �������� ������ �ٲ�
        changeDirectionTime -= Time.deltaTime;
        if (changeDirectionTime <= 0)
        {
            ChangeDirection();
        }

        // ȸ�� �� �ش� �������� �̵�(ȸ�� �� forward �������� �̵�)
        // ��ǥ ȸ�� ���
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
    /// ������ ���� ���� ���ƿ��� �޼���
    /// </summary>
    public void ReturnToStart()
    {
        animator.SetBool(MonsterAnimationName.isRun, true);

        Vector3 directionToStart = new Vector3(spawner.transform.position.x, transform.position.y, spawner.transform.position.z) - transform.position;
        directionToStart = directionToStart.normalized;

        Quaternion targetRotation = Quaternion.LookRotation(directionToStart);
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * rotationSpeed);

        // ȸ�� �� �ش� �������� �̵�
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

        // �÷��̾� hp -= atk_power;
        Collider[] targetsInRange = Physics.OverlapSphere(transform.position, atkRange);

        foreach (Collider target in targetsInRange)
        {
            if (target.CompareTag(MonsterTag.Player) && !IsTargetInAttackRange(target.transform))
            {
                isTargetInSight = false;
                ChangeState(new WaitForAttackState(this));
                return;
            }

            // ������ �Ÿ� Ȯ��
            if (target.CompareTag(MonsterTag.Player) && IsTargetInAttackRange(target.transform))
            {
                Debug.Log("���� �����ȿ� ���ͼ� ����!");
                isTargetInSight = true;
                RpcSetTrigger(MonsterAnimationName.isAttack);
                attackTimer = atkInterval;
            }
        }
    }

    protected virtual void AttackSound() { }   

    /// <summary>
    /// ������ �����̴� ������ �������� �����ϴ� �޼���
    /// </summary>
    public void ChangeDirection()
    {
        float angle = Random.Range(0f, 360f);
        movedirection = new Vector3(Mathf.Cos(angle), 0, Mathf.Sin(angle)).normalized;

        changeDirectionTime = Random.Range(changeDirectionTimeMin, changeDirectionTimeMax);
    }

    /// <summary>
    /// Ÿ���� ���ݹ����� ���� �ִ��� Ȯ���ϴ� �޼���
    /// </summary>
    /// <param name="target"></param>
    /// <returns></returns>
    public bool IsTargetInAttackRange(Transform target)
    {
        Vector3 directionToTarget = (target.position - transform.position).normalized;
        float distanceToTarget = Vector3.Distance(transform.position, target.position);

        // �Ÿ��� ���� Ȯ��
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
    /// ���� �޾��� �� �����ϴ� �޼���(�ִϸ��̼� �̺�Ʈ Ʈ���� ���)
    /// </summary>
    /// <param name="atk">������ ����� ���ݷ�</param>
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
    /// Ÿ���� �ǹ��ȿ� ������ Ȯ���ϴ� �޼���
    /// </summary>
    private void IsTargetInsideBuilding(Transform target)
    {
        if (target != null) isTargetBuildingEnter = (target.GetComponent<PenguinBody>().isBuildingEnter) ? true : false;

        if (isTargetBuildingEnter)
        {
            ChangeState(new IdleState(this));
        }
    }

    #region �ִϸ��̼� �̺�Ʈ Ʈ���ſ� ������ �޼���
    /// <summary>
    /// ���Ͱ� �����ϰ� ���ظ� ������ ���� �ִϸ��̼� �̺�Ʈ�� ������Ŵ
    /// </summary>
    public void OnAttackHit()
    {
        AttackSound();

        // Ÿ���� ���� ���� �ְ� ������ ���� ��ȿ�� ���� ���ظ� �����մϴ�.
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

    #region ���� ���� �޼���
    [ClientRpc]
    public void RpcSetTrigger(string trigger)
    {
        animator.SetTrigger(trigger);
    }
    #endregion

    /// <summary>
    /// ���� ����� Ÿ���� ã�� �޼���
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
    /// ���ο� Ÿ���� �����ϴ� �޼���
    /// </summary>
    /// <param name="newTarget"></param>
    public void SetTarget(Transform newTarget) => this.target = newTarget;

    /// <summary>
    /// ������ ���� ������ �������� Ȯ���ϴ� �޼���
    /// </summary>
    /// <returns></returns>
    public bool IsOutOfSpawnRange() => Vector3.Distance(transform.position, startPosition) > detectRange;

    /// <summary>
    /// ���� ���� �޼���
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
    /// �׾��� �� �ִϸ��̼� ���� �� ���� �ϴ� �ڷ�ƾ
    /// </summary>
    /// <returns></returns>
    public IEnumerator DestroyObject()
    {
        yield return waitingDieAnimation;

        NetworkObjectManager.Instance.DestroyNetworkObject(this.gameObject);
    }

    /// <summary>
    /// �������� �� �ִϸ��̼� ���� �� false�� ��ȯ �ϴ� �ڷ�ƾ 
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
                jumpVelocity.y = 5f; // ���� �ӵ� ���� (�� ���� �����Ͽ� ���� ���̸� ����)
                rb.velocity = jumpVelocity;
            }
        }
    }

    #region Action State
    /// <summary>
    /// Ÿ���� ���󰡴� �޼���
    /// </summary>
    public void FollowToTarget()
    {
        if (target == null) return;

        Vector3 directionToStart = new Vector3(target.position.x, transform.position.y, target.position.z) - transform.position;
        directionToStart = directionToStart.normalized;

        Quaternion targetRotation = Quaternion.LookRotation(directionToStart);
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * rotationSpeed);

        // �÷��̾� �������� �̵�
        Vector3 dir = (target.position - transform.position).normalized;
        transform.position += dir * moveSpeed * Time.deltaTime;
        animator.SetBool(MonsterAnimationName.isRun, true);

        Jump();
    }

    /// <summary>
    /// �ݴ� �������� Ÿ�ٿ��� �ָ� �������� �޼���
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
