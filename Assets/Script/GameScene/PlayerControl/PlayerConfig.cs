using UnityEngine;

/// <summary>
/// 玩家配置数据（ScriptableObject）
/// 在 Unity 中右键 → Create → RPG → PlayerConfig 创建配置资产
/// </summary>
[CreateAssetMenu(fileName = "PlayerConfig", menuName = "RPG/PlayerConfig", order = 3)]
public class PlayerConfig : ScriptableObject
{
    [Header("移动")]
    [Tooltip("玩家移动速度（单位/秒）")]
    public float moveSpeed = 5f;

    [Header("血量")]
    [Tooltip("玩家满血血量")]
    public float maxHP = 100f;
}
