using UnityEngine;

/// <summary>
/// 子弹脚本 —— Rigidbody.velocity 飞行，OnTriggerEnter 检测碰撞，命中后回池
///
/// 对象池使用流程：
///   var obj = ObjectPool.Instance.Pop(bulletPrefabKey);
///   obj.transform.position = muzzlePos;
///   obj.GetComponent&lt;Bullet&gt;().Fire(direction, bulletPrefabKey);
/// </summary>
[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(Collider))]
public class Bullet : MonoBehaviour
{
    [Header("配置")]
    [SerializeField] private string configKey = "BulletConfig";  // BulletConfig 的 Addressable 键名

    private Rigidbody rb;
    private float speed;
    private float damage;
    private string poolKey;                                      // 对象池 key（子弹预制体）

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        rb.useGravity = false;
        rb.isKinematic = false;
        rb.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
    }

    void OnDisable()
    {
        rb.velocity = Vector3.zero;
    }

    /// <summary>
    /// 发射子弹（外部在 Pop 后调用）
    /// </summary>
    /// <param name="direction">飞行方向</param>
    /// <param name="poolKey">对象池 key，对应子弹预制体，Push 时也用这个 key</param>
    public void Fire(Vector3 direction, string poolKey)
    {
        this.poolKey = poolKey;

        // 加载子弹配置（独立于预制体的 Addressable key）
        BulletConfig config = AddressableManager.Instance.LoadRes<BulletConfig>(configKey);
        speed = config != null ? config.speed : 10f;
        damage = config != null ? config.damage : 10f;

        direction.y = 0f;
        if (direction.sqrMagnitude < 0.001f) direction = Vector3.forward;
        rb.velocity = direction.normalized * speed;
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.gameObject == gameObject) return;
        if (other.CompareTag("Player")) return;

        if (other.TryGetComponent<Master>(out var master))
        {
            master.TakeDamage(damage);
        }
        else if (!HasTag(other.transform, "Env"))
        {
            return;
        }

        ObjectPool.Instance.Push(poolKey, gameObject);
    }

    private static bool HasTag(Transform t, string tag)
    {
        if (t.CompareTag(tag)) return true;
        if (t.parent != null && t.parent.CompareTag(tag)) return true;
        return false;
    }
}
