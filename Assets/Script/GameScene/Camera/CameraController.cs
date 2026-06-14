using UnityEngine;

/// <summary>
/// 摄像机控制脚本 —— 获取主相机引用，偏移跟随目标 + 平滑移动
/// </summary>
public class CameraController : MonoBehaviour
{
    [Header("配置")]
    [SerializeField] private CameraConfig config;            // 相机配置资产（ScriptableObject）

    [Header("相机引用")]
    [SerializeField] private Camera targetCamera;            // 要控制的主相机

    [Header("跟随目标")]
    [SerializeField] private Transform target;              // 跟随目标（玩家）

    // 运行时从 config 读取的值
    private Vector3 offset;
    private float followSpeed;

    void Start()
    {
        // 从配置资产读取参数，未配置则使用默认值
        if (config != null)
        {
            offset = config.offset;
            followSpeed = config.followSpeed;
        }
        else
        {
            offset = new Vector3(0, 10, 0);
            followSpeed = 5f;
            Debug.LogWarning("CameraController: CameraConfig 未配置，使用默认值");
        }

        // 未手动指定则自动获取主相机
        if (targetCamera == null)
        {
            targetCamera = Camera.main;
        }

        // 俯视角：相机固定垂直向下
        if (targetCamera != null)
        {
            targetCamera.transform.rotation = Quaternion.Euler(90f, 0f, 0f);
        }
    }

    void LateUpdate()
    {
        if (targetCamera == null || target == null)
        {
            return;
        }

        // 俯视角：相机在目标正上方，只跟随 X/Z 平面位置，Y 保持固定高度
        Vector3 desiredPosition = new Vector3(
            target.position.x + offset.x,
            target.position.y + offset.y,
            target.position.z + offset.z
        );

        // 平滑跟随
        targetCamera.transform.position = Vector3.Lerp(
            targetCamera.transform.position,
            desiredPosition,
            followSpeed * Time.deltaTime
        );
    }

    /// <summary>
    /// 设置跟随目标
    /// </summary>
    public void SetTarget(Transform newTarget)
    {
        target = newTarget;
    }

    /// <summary>
    /// 设置要控制的相机
    /// </summary>
    public void SetCamera(Camera cam)
    {
        targetCamera = cam;
    }

}
