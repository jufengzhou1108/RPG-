using UnityEngine;

/// <summary>
/// 游戏场景的配置数据（ScriptableObject）
/// 在 Unity 中右键 → Create → RPG/GameSceneConfig 创建配置资产
/// </summary>
[CreateAssetMenu(fileName = "GameSceneConfig", menuName = "RPG/GameSceneConfig", order = 1)]
public class GameSceneConfig : ScriptableObject
{
    [Header("玩家初始化")]
    [Tooltip("玩家名称")]
    public string playerName = "Player";

    [Tooltip("玩家进入场景时的出生位置")]
    public Vector3 playerSpawnPosition = Vector3.zero;

    [Tooltip("玩家进入场景时的朝向")]
    public Vector3 playerSpawnRotation = Vector3.zero;

    [Header("场景配置")]
    [Tooltip("场景默认加载的环境预制体列表（Addressable 键名）")]
    public string[] defaultEnvPrefabs;
}
