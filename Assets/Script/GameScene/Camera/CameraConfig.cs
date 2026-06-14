using UnityEngine;

/// <summary>
/// 相机配置数据（ScriptableObject）
/// 在 Unity 中右键 → Create → RPG/CameraConfig 创建配置资产
/// 由 CameraController 直接引用，无需场景管理器转发
/// </summary>
[CreateAssetMenu(fileName = "CameraConfig", menuName = "RPG/CameraConfig", order = 2)]
public class CameraConfig : ScriptableObject
{
    [Header("相机偏移")]
    [Tooltip("摄像机相对跟随目标的偏移（俯视角：仅 Y 轴高度，垂直向下）")]
    public Vector3 offset = new Vector3(0, 10, 0);

    [Header("跟随平滑")]
    [Tooltip("摄像机跟随平滑速度（值越大越快）")]
    public float followSpeed = 5f;
}
