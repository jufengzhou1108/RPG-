using UnityEngine;

/// <summary>
/// 开始场景的配置数据（ScriptableObject）
/// 在 Unity 中右键 → Create → RPG/BeginSceneConfig 创建配置资产
/// </summary>
[CreateAssetMenu(fileName = "BeginSceneConfig", menuName = "RPG/BeginSceneConfig", order = 0)]
public class BeginSceneConfig : ScriptableObject
{
    [Header("场景配置")]
    [Tooltip("点击'开始游戏'后加载的场景名称")]
    public string nextSceneName = "Main";
}
