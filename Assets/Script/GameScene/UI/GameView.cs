using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// GameView 显示事件（EventCenter 用，无参数）
/// </summary>
public struct GameViewShowEvent { }

/// <summary>
/// GameView 隐藏事件（EventCenter 用，无参数）
/// </summary>
public struct GameViewHideEvent { }

/// <summary>
/// 玩家血量更新事件 —— PlayerController 受击或 GameView 显示时推送
/// </summary>
public struct PlayerHPUpdateEvent
{
    public float currentHP;
    public float maxHP;
}

/// <summary>
/// 主游戏界面 —— 游戏场景的 HUD / 核心交互层
///
/// 预制体结构：
///   GameView (本脚本)
///     ├─ BtnTask (Button)              ← 任务按钮
///     ├─ HPBg (Image)                  ← 血条背景
///     │   └─ HPFill (Image)            ← 血条填充（Image.Type = Filled, FillMethod = Horizontal）
///     └─ ...
/// </summary>
public class GameView : MonoBehaviour
{
    [Header("按钮")]
    [SerializeField] private Button btnTask;

    [Header("血条")]
    [SerializeField] private Image hpFill;                      // 血条填充（Filled / Horizontal）

    void Awake()
    {
        // 在 OnEnable 触发 GameViewShowEvent 之前先订阅血量事件，
        // 确保 PlayerController 响应 GameViewShowEvent 推送血量时 GameView 已就绪
        EventCenter.Instance.AddListener<PlayerHPUpdateEvent>(OnHPUpdate);
    }

    void OnEnable()
    {
        EventCenter.Instance.EventTrigger(new GameViewShowEvent());
    }

    void OnDisable()
    {
        EventCenter.Instance.EventTrigger(new GameViewHideEvent());
    }

    void Start()
    {
        if (btnTask != null)
        {
            btnTask.onClick.AddListener(OnTaskClicked);
        }
    }

    #region 血条

    /// <summary>
    /// 收到血量更新事件 → 刷新血条填充比例
    /// fillAmount = visibleWidth / bgWidth = currentHP / maxHP
    /// </summary>
    private void OnHPUpdate(PlayerHPUpdateEvent e)
    {
        if (hpFill != null)
        {
            hpFill.fillAmount = e.maxHP > 0f ? e.currentHP / e.maxHP : 0f;
        }
    }

    #endregion

    private void OnTaskClicked()
    {
        ViewManager.Instance.Hide<GameView>();
        ViewManager.Instance.Show<TaskView>();
    }

    void OnDestroy()
    {
        EventCenter.Instance.RemoveListener<PlayerHPUpdateEvent>(OnHPUpdate);

        if (btnTask != null)
        {
            btnTask.onClick.RemoveListener(OnTaskClicked);
        }
    }
}
