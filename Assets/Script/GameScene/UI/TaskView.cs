using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 任务界面 —— 包含左侧任务列表、右侧任务详情、退出按钮
///
/// 预制体结构：
///   TaskView (本脚本)
///     ├─ TaskList (TaskList 组件)       ← 左侧，虚拟化滚动列表
///     ├─ TalkDetails (TalkDetails 组件) ← 右侧，任务详情面板
///     └─ BtnExit (Button)              ← 退出按钮，点后回 GameView
///
/// 流程：
///   玩家点击列表项 → TaskList.onTaskClicked(taskId)
///   → TaskView 收到 taskId → TalkDetails.Show(taskId) → TaskManager 按需加载配置并显示
///   玩家点击退出 → Hide<TaskView> + Show<GameView>
/// </summary>
public class TaskView : MonoBehaviour
{
    [Header("子组件")]
    [SerializeField] private TaskList taskList;           // 任务列表
    [SerializeField] private TalkDetails talkDetails;     // 任务详情

    [Header("按钮")]
    [SerializeField] private Button btnExit;              // 退出按钮

    void Start()
    {
        // 列表项点击 → 详情面板显示
        if (taskList != null)
        {
            taskList.onTaskClicked.AddListener(OnTaskSelected);
        }

        // 退出按钮 → 回到主界面
        if (btnExit != null)
        {
            btnExit.onClick.AddListener(OnExitClicked);
        }
    }

    /// <summary>
    /// 退出按钮点击：关闭任务界面，返回主游戏界面
    /// </summary>
    private void OnExitClicked()
    {
        ViewManager.Instance.Hide<TaskView>();
        ViewManager.Instance.Show<GameView>();
    }

    void OnEnable()
    {
        // 界面激活时刷新列表（从其它界面切回来）
        if (taskList != null)
        {
            taskList.Refresh();
        }
    }

    /// <summary>
    /// 列表项被点击时，将任务 ID 传给详情面板
    /// </summary>
    private void OnTaskSelected(string taskId)
    {
        if (talkDetails != null)
        {
            talkDetails.Show(taskId);
        }
    }

    void OnDestroy()
    {
        if (taskList != null)
        {
            taskList.onTaskClicked.RemoveListener(OnTaskSelected);
        }

        if (btnExit != null)
        {
            btnExit.onClick.RemoveListener(OnExitClicked);
        }
    }
}
