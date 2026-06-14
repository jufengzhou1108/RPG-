using UnityEngine;

/// <summary>
/// 怪物配置（ScriptableObject）
/// 在 Project 窗口中右键 → Create → RPG → MonsterConfig 创建
/// 注册到 Addressables，键名为怪物 ID（如 "Slime"）
/// </summary>
[CreateAssetMenu(fileName = "MonsterConfig", menuName = "RPG/MonsterConfig", order = 6)]
public class MonsterConfig : ScriptableObject
{
    [Header("基础属性")]
    [Tooltip("最大生命值")]
    public float maxHP = 100f;

    [Tooltip("攻击力")]
    public float attackPower = 10f;

    [Header("追击")]
    [Tooltip("追击范围（米）")]
    public float chaseRange = 5f;

    [Tooltip("移动速度（米/秒）")]
    public float moveSpeed = 3f;

    [Header("攻击")]
    [Tooltip("攻击范围（米）")]
    public float attackRange = 1f;

    [Tooltip("攻击间隔（秒）")]
    public float attackInterval = 1.5f;
}
