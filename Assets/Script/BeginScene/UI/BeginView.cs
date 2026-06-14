using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

/// <summary>
/// 开始界面视图 —— 提供"开始游戏"和"退出游戏"按钮
/// </summary>
public class BeginView : MonoBehaviour
{
    [Header("按钮引用")]
    [SerializeField] private Button btnStartGame;   // 开始游戏按钮
    [SerializeField] private Button btnExitGame;    // 退出游戏按钮

    [Header("配置")]
    [SerializeField] private BeginSceneConfig config;  // ScriptableObject 配置资产

    void Start()
    {
        // 绑定按钮点击事件
        if (btnStartGame != null)
        {
            btnStartGame.onClick.AddListener(OnStartGameClicked);
        }

        if (btnExitGame != null)
        {
            btnExitGame.onClick.AddListener(OnExitGameClicked);
        }
    }

    /// <summary>
    /// 开始游戏按钮点击 —— 加载下一个场景
    /// </summary>
    private void OnStartGameClicked()
    {
        if (config != null)
        {
            SceneManager.LoadScene(config.nextSceneName);
        }
        else
        {
            Debug.LogError("BeginSceneConfig 未配置，请在 Inspector 中拖入配置资产");
        }
    }

    /// <summary>
    /// 退出游戏按钮点击 —— 退出应用程序
    /// </summary>
    private void OnExitGameClicked()
    {
        Application.Quit();

        // 编辑器中退出播放模式
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#endif
    }

    void OnDestroy()
    {
        // 移除监听，防止内存泄漏
        if (btnStartGame != null)
        {
            btnStartGame.onClick.RemoveListener(OnStartGameClicked);
        }

        if (btnExitGame != null)
        {
            btnExitGame.onClick.RemoveListener(OnExitGameClicked);
        }
    }
}
