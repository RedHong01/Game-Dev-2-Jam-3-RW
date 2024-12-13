using UnityEngine;
using System.Collections;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
public class Enemy : Lives
{
    public enum State { Idle, Chasing, Attacking }; // 敌人的状态：闲置、追逐、攻击
    State currentState;

    public ParticleSystem deathEffect; // 死亡特效

    private NavMeshAgent pathfinder; // 导航代理
    private Transform target; // 玩家目标
    private Lives targetEntity; // 玩家生命系统

    private float attackDistanceThreshold = 0.5f; // 攻击的距离阈值
    private float timeBetweenAttacks = 1f; // 攻击间隔
    private float damage = 1f; // 每次攻击的伤害

    private float nextAttackTime; // 下一次攻击时间
    private float myCollisionRadius; // 敌人碰撞体半径
    private float targetCollisionRadius; // 玩家碰撞体半径

    private bool hasTarget; // 是否存在目标

    protected override void Start()
    {
        base.Start();

        pathfinder = GetComponent<NavMeshAgent>();

        // 立即寻找目标
        FindTarget();

        // 启动实时更新逻辑
        StartCoroutine(UpdateTarget()); // 启动实时目标更新
        StartCoroutine(UpdatePath());   // 启动导航路径更新协程
    }

    public override void TakeHit(float damage, Vector3 hitPoint, Vector3 hitDirection)
    {
        if (damage >= health && deathEffect != null)
        {
            // 播放死亡特效
            var effect = Instantiate(deathEffect.gameObject, hitPoint, Quaternion.FromToRotation(Vector3.forward, hitDirection));
            Destroy(effect, deathEffect.main.startLifetime.constant);
        }

        base.TakeHit(damage, hitPoint, hitDirection); // 调用基类的受击逻辑
    }

    private void OnTargetDeath()
    {
        hasTarget = false;
        target = null;
        targetEntity = null;
        currentState = State.Idle; // 目标死亡后进入闲置状态
    }

    void Update()
    {
        if (hasTarget && target != null && Time.time > nextAttackTime)
        {
            float sqrDstToTarget = (target.position - transform.position).sqrMagnitude;
            float attackRadius = Mathf.Pow(attackDistanceThreshold + myCollisionRadius + targetCollisionRadius, 2);

            if (sqrDstToTarget < attackRadius)
            {
                nextAttackTime = Time.time + timeBetweenAttacks;
                StartCoroutine(Attack()); // 进入攻击行为
            }
        }
    }

    IEnumerator Attack()
    {
        if (target == null) yield break; // 确保目标存在

        currentState = State.Attacking;
        pathfinder.enabled = false; // 暂停导航

        Vector3 originalPosition = transform.position;
        Vector3 dirToTarget = (target.position - transform.position).normalized;
        Vector3 attackPosition = target.position - dirToTarget * myCollisionRadius;

        float attackSpeed = 3f;
        float percent = 0f;

        bool hasAppliedDamage = false;

        while (percent <= 1f)
        {
            if (target == null) break; // 确保目标仍然有效

            if (percent >= 0.5f && !hasAppliedDamage)
            {
                hasAppliedDamage = true;
                if (targetEntity != null)
                {
                    targetEntity.TakeDamage(damage); // 对目标造成伤害
                }
            }

            percent += Time.deltaTime * attackSpeed;
            float interpolation = (-Mathf.Pow(percent, 2) + percent) * 4; // 插值曲线
            transform.position = Vector3.Lerp(originalPosition, attackPosition, interpolation);

            yield return null;
        }

        currentState = State.Chasing; // 恢复追逐状态
        pathfinder.enabled = true; // 重新启用导航
    }

    IEnumerator UpdatePath()
{
    float refreshRate = 0.25f;

    while (true)
    {
        if (hasTarget && target != null && currentState == State.Chasing && !dead && currentState != State.Attacking)
        {
            Vector3 dirToTarget = (target.position - transform.position).normalized;
            Vector3 targetPosition = target.position - dirToTarget * (myCollisionRadius + targetCollisionRadius + attackDistanceThreshold / 2);

            // 确保 NavMeshAgent 可用
            if (pathfinder.isOnNavMesh && pathfinder.isActiveAndEnabled)
            {
                pathfinder.SetDestination(targetPosition); // 设置导航目标位置
            }
            else
            {
                Debug.LogWarning("NavMeshAgent is not on NavMesh or is not active.");
            }
        }
        yield return new WaitForSeconds(refreshRate);
    }
}

    IEnumerator UpdateTarget()
{
    float targetRefreshRate = 0.25f; // 更频繁地检查目标

    while (true)
    {
        if (GameManager.IsPlayerAlive)
        {
            FindTarget(); // 动态寻找目标
        }
        else
        {
            // 玩家不存在，将敌人状态设置为 Idle
            hasTarget = false;
            target = null;
            currentState = State.Idle;

            Debug.Log("Enemy set to Idle: No player alive.");
        }

        yield return new WaitForSeconds(targetRefreshRate);
    }
}

private void FindTarget()
{
    GameObject playerObject = GameObject.FindGameObjectWithTag("Player");
    if (playerObject != null)
    {
        target = playerObject.transform;
        hasTarget = true;

        targetEntity = target.GetComponent<Lives>();
        if (targetEntity != null)
        {
            targetEntity.OnDeath += OnTargetDeath; // 确保目标的死亡事件已注册
        }

        myCollisionRadius = GetComponent<CapsuleCollider>().radius;
        targetCollisionRadius = target.GetComponent<CapsuleCollider>().radius;

        currentState = State.Chasing; // 设置状态为追逐
        Debug.Log("Enemy found target: " + target.name);
    }
    else
    {
        hasTarget = false;
        target = null;
        targetEntity = null;
        currentState = State.Idle; // 没有目标时进入闲置状态
        Debug.Log("Enemy target lost. Searching...");
    }
}

    
}