using System;
using UnityEngine;

/// <summary>
/// 怪物控制脚本
///
/// 通过范围检测（OverlapSphere）感知玩家：
///   Normal — 原地不动，绿色材质。检测到玩家进入追击范围 → Chase
///   Chase  — 追向玩家，红色材质。进入攻击范围 → 停步攻击；超出追击范围 → Normal
///
/// 对象池适配：
///   Awake  → 获取组件引用 + 加载配置
///   OnEnable → 重置 HP、状态（每次从池中取出时）
///   死亡   → 触发 OnDeath 事件，由 MasterPoint 回收入池
/// </summary>
[RequireComponent(typeof(CharacterController))]
public class Master : MonoBehaviour
{
    [Header("Addressable 键名")]
    [SerializeField] private string configKey = "Slime";
    [SerializeField] private string greenMatKey = "Green";
    [SerializeField] private string redMatKey = "Red";

    private MonsterConfig config;
    private CharacterController cc;
    private MeshRenderer meshRenderer;
    private float currentHP;
    private float attackTimer;
    private float detectTimer;
    private Transform cachedPlayer;
    private Vector3 wanderDir;
    private float wanderTimer;

    public enum State { Normal, Chase }
    private State state;

    /// <summary>
    /// 死亡事件 —— MasterPoint 订阅此事件回收怪物
    /// </summary>
    public event Action<Master> OnDeath;

    void Awake()
    {
        cc = GetComponent<CharacterController>();
        meshRenderer = GetComponentInChildren<MeshRenderer>();

        config = AddressableManager.Instance.LoadRes<MonsterConfig>(configKey);
        if (config == null)
        {
            Debug.LogError($"Master: 找不到怪物配置 [{configKey}]");
        }
    }

    void OnEnable()
    {
        currentHP = config != null ? config.maxHP : 100f;
        attackTimer = 0f;
        detectTimer = 0f;
        cachedPlayer = null;
        wanderTimer = 0f;
        EnterState(State.Normal);
    }

    void Update()
    {
        if (config == null) return;

        if (attackTimer > 0f)
        {
            attackTimer -= Time.deltaTime;
        }

        float detectInterval = state == State.Normal ? 1f : 0.2f;
        detectTimer -= Time.deltaTime;
        if (detectTimer <= 0f)
        {
            detectTimer = detectInterval;
            cachedPlayer = DetectPlayer(config.chaseRange);
        }

        switch (state)
        {
            case State.Normal:
                Wander();

                if (cachedPlayer != null)
                {
                    EnterState(State.Chase);
                }
                break;

            case State.Chase:
                if (cachedPlayer == null)
                {
                    EnterState(State.Normal);
                    break;
                }

                float sqrDist = (transform.position - cachedPlayer.position).sqrMagnitude;
                float attackSqr = config.attackRange * config.attackRange;

                if (sqrDist <= attackSqr)
                {
                    cc.SimpleMove(Vector3.zero);
                    TryAttack(cachedPlayer);
                }
                else
                {
                    Vector3 toPlayer = cachedPlayer.position - transform.position;
                    toPlayer.y = 0f;
                    if (toPlayer.sqrMagnitude > 0.01f)
                    {
                        cc.SimpleMove(toPlayer.normalized * config.moveSpeed);
                    }
                }
                break;
        }
    }

    private void Wander()
    {
        wanderTimer -= Time.deltaTime;
        if (wanderTimer <= 0f)
        {
            wanderTimer = UnityEngine.Random.Range(1f, 3f);
            float angle = UnityEngine.Random.Range(0f, 360f);
            wanderDir = new Vector3(Mathf.Cos(angle * Mathf.Deg2Rad), 0f, Mathf.Sin(angle * Mathf.Deg2Rad));
        }

        cc.SimpleMove(wanderDir * 0.5f);
    }

    private Transform DetectPlayer(float radius)
    {
        Collider[] hits = Physics.OverlapSphere(transform.position, radius);
        foreach (Collider hit in hits)
        {
            if (hit.CompareTag("Player"))
            {
                return hit.transform;
            }
        }
        return null;
    }

    private void TryAttack(Transform player)
    {
        if (attackTimer > 0f) return;

        Vector3 toPlayer = player.position - transform.position;
        toPlayer.y = 0f;

        if (toPlayer.sqrMagnitude < 0.01f) return;
        if (Vector3.Dot(transform.forward, toPlayer.normalized) < 0.5f) return;

        Collider[] hits = Physics.OverlapSphere(transform.position, config.attackRange);
        foreach (Collider hit in hits)
        {
            if (hit.CompareTag("Player"))
            {
                Debug.Log($"怪物对玩家造成 {config.attackPower} 点伤害");
                attackTimer = config.attackInterval;
                break;
            }
        }
    }

    private void EnterState(State newState)
    {
        state = newState;

        switch (newState)
        {
            case State.Normal:
                LoadMaterial(greenMatKey);
                break;
            case State.Chase:
                LoadMaterial(redMatKey);
                break;
        }
    }

    private void LoadMaterial(string matKey)
    {
        if (meshRenderer == null) return;

        Material mat = AddressableManager.Instance.LoadRes<Material>(matKey);
        if (mat != null)
        {
            meshRenderer.material = mat;
        }
    }

    #region 外部接口

    public void TakeDamage(float damage)
    {
        if (damage <= 0 || currentHP <= 0) return;

        currentHP -= damage;

        if (currentHP <= 0)
        {
            Die();
        }
    }

    private void Die()
    {
        OnDeath?.Invoke(this);
    }

    void OnTriggerEnter(Collider other)
    {
        // 子弹命中由 Bullet.OnTriggerEnter 统一处理（伤害 + 回池）
    }

    public float CurrentHP => currentHP;
    public float MaxHP => config != null ? config.maxHP : 100f;
    public State CurrentState => state;

    #endregion
}
