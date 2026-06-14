using UnityEngine;

/// <summary>
/// 游戏场景管理器 —— 管理相机、环境、玩家初始化
/// 进入场景时根据 GameSceneConfig 配置完成场景初始化
/// </summary>
public class GameSceneManager : MonoBehaviour
{
    [Header("配置")]
    [SerializeField] private GameSceneConfig config;            // 场景配置资产

    [Header("玩家配置")]
    [SerializeField] private string playerPrefabName = "Player"; // 玩家预制体 Addressable 键名

    private CameraController cameraController; // 摄像机控制脚本（GetComponent 获取）
    private EnvManager envManager;             // 环境管理器（GetComponent 获取）
    private GameObject playerInstance;         // 玩家实例引用

    void Start()
    {
        if (config == null)
        {
            Debug.LogError("GameSceneManager: GameSceneConfig 未配置，请在 Inspector 中拖入配置资产");
            return;
        }

        // 获取同物体上的组件
        cameraController = GetComponent<CameraController>();
        envManager = GetComponent<EnvManager>();

        // 0. 初始化任务管理器
        InitTask();

        // 0.5 订阅对话事件 — 控制 GameView 显隐
        InitDialogueEvents();

        // 1. 初始化摄像机 — 传递配置和主相机引用
        InitCamera();

        // 2. 加载玩家
        LoadPlayer();

        // 3. 加载初始环境
        LoadDefaultEnv();

        // 4. 加载主界面
        ViewManager.Instance.Show<GameView>();
    }

    /// <summary>
    /// 初始化任务管理器：加载进度存档（玩家已接受的任务）
    /// 任务配置由 TaskManager 按需通过 Addressables 加载
    /// </summary>
    private void InitTask()
    {
        TaskManager.Instance.Init();
    }

    /// <summary>
    /// 订阅对话开始/结束事件，控制 GameView 的显隐
    /// 对话开始时隐藏，对话结束时显示
    /// </summary>
    private void InitDialogueEvents()
    {
        EventCenter.Instance.AddListener<DialogueStartEvent>(OnDialogueStart);
        EventCenter.Instance.AddListener<DialogueEndEvent>(OnDialogueEnd);
    }

    private void OnDialogueStart(DialogueStartEvent _)
    {
        ViewManager.Instance.Hide<GameView>();
    }

    private void OnDialogueEnd(DialogueEndEvent _)
    {
        ViewManager.Instance.Show<GameView>();
    }

    /// <summary>
    /// 初始化摄像机：将主相机引用传给相机脚本
    /// 相机配置由 CameraController 自己的 CameraConfig 资产提供，无需在此转发
    /// </summary>
    private void InitCamera()
    {
        if (cameraController == null) return;

        // 获取主相机并传递给相机脚本
        Camera mainCam = Camera.main;
        if (mainCam != null)
        {
            cameraController.SetCamera(mainCam);
        }
    }

    /// <summary>
    /// 通过 AddressableManager 同步加载玩家预制体，放置在配置的出生点
    /// 同步加载确保 PlayerController 在 GameView 之前完成事件订阅
    /// </summary>
    private void LoadPlayer()
    {
        GameObject prefab = AddressableManager.Instance.LoadRes<GameObject>(playerPrefabName);
        if (prefab == null)
        {
            Debug.LogError($"GameSceneManager: 加载玩家预制体 \"{playerPrefabName}\" 失败");
            return;
        }

        // 在配置的出生点实例化玩家
        Vector3 spawnPos = config.playerSpawnPosition;
        Quaternion spawnRot = Quaternion.Euler(config.playerSpawnRotation);

        playerInstance = Instantiate(prefab, spawnPos, spawnRot);
        playerInstance.name = config.playerName;

        // 将玩家设置为相机跟随目标
        if (cameraController != null)
        {
            cameraController.SetTarget(playerInstance.transform);
        }
    }

    /// <summary>
    /// 根据配置加载默认环境预制体
    /// </summary>
    private void LoadDefaultEnv()
    {
        if (envManager == null) return;

        if (config.defaultEnvPrefabs == null || config.defaultEnvPrefabs.Length == 0)
        {
            Debug.Log("GameSceneManager: 没有需要加载的默认环境");
            return;
        }

        foreach (string envName in config.defaultEnvPrefabs)
        {
            if (!string.IsNullOrEmpty(envName))
            {
                envManager.LoadEnv(envName);
            }
        }
    }

    /// <summary>
    /// 获取玩家实例
    /// </summary>
    public GameObject GetPlayer()
    {
        return playerInstance;
    }

    void OnDestroy()
    {
        EventCenter.Instance.RemoveListener<DialogueStartEvent>(OnDialogueStart);
        EventCenter.Instance.RemoveListener<DialogueEndEvent>(OnDialogueEnd);

        // 场景销毁时清理玩家实例
        if (playerInstance != null)
        {
            Destroy(playerInstance);
        }
    }
}
