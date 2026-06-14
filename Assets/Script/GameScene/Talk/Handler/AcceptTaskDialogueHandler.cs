using UnityEngine;

/// <summary>
/// 接受任务对话处理器 —— 从 param 取任务 ID，调 TaskManager 接受任务
/// param 为 "-1" 或空 → 跳过（拒绝任务）
/// </summary>
public class AcceptTaskDialogueHandler : DialogueHandlerBase
{
    [RuntimeInitializeOnLoadMethod]
    static void Init()
    {
        Register("接受任务", new AcceptTaskDialogueHandler());
    }

    public override string Process(DialogueAction action)
    {
        if (!string.IsNullOrEmpty(action.param) && action.param != "-1")
        {
            TaskManager.Instance.AcceptTask(action.param);
        }

        return null; // 不跳转，由后续动作决定下一段对话
    }
}
