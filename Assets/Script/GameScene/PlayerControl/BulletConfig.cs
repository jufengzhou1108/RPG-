using UnityEngine;

/// <summary>
/// 子弹配置（ScriptableObject）
/// 在 Project 窗口中右键 → Create → RPG → BulletConfig 创建
/// 注册到 Addressables，键名与子弹预制体一致（如 "Bullet"）
/// </summary>
[CreateAssetMenu(fileName = "BulletConfig", menuName = "RPG/BulletConfig", order = 7)]
public class BulletConfig : ScriptableObject
{
    [Header("基础属性")]
    [Tooltip("子弹飞行速度（米/秒）")]
    public float speed = 10f;

    [Tooltip("子弹伤害")]
    public float damage = 10f;
}
