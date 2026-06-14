using System.Collections.Generic;

/// <summary>
/// 对话处理基类 —— 派生类通过静态构造函数自动注册
/// 注册的 ID 与任务 ID（即 DialogueEntry.dialogueType）一致
///
/// 使用方式：
///   public class MyDialogueHandler : DialogueHandlerBase
///   {
///       static MyDialogueHandler()
///       {
///           Register("my_task_id", new MyDialogueHandler());
///       }
///
///       public override string Process(DialogueEntry entry, int optionIndex)
///       {
///           // 处理对话逻辑，返回下一条对话的 ID
///           // 返回 null 或空字符串表示对话结束
///       }
///   }
/// </summary>
public abstract class DialogueHandlerBase
{
    private static Dictionary<string, DialogueHandlerBase> handlerDic = new Dictionary<string, DialogueHandlerBase>();

    /// <summary>
    /// 派生类在静态构造函数中调用此方法注册自身
    /// </summary>
    /// <param name="id">处理器 ID（与任务 ID / dialogueType 一致）</param>
    /// <param name="handler">处理器实例</param>
    protected static void Register(string id, DialogueHandlerBase handler)
    {
        if (handlerDic.ContainsKey(id))
        {
            UnityEngine.Debug.LogWarning($"DialogueHandlerBase: ID [{id}] 已注册，将被覆盖。");
        }
        handlerDic[id] = handler;
    }

    /// <summary>
    /// 根据 ID 获取对话处理器
    /// </summary>
    public static DialogueHandlerBase GetHandler(string id)
    {
        if (string.IsNullOrEmpty(id)) return null;
        handlerDic.TryGetValue(id, out var handler);
        return handler;
    }

    /// <summary>
    /// 处理一个对话动作，返回下一条对话的 ID（null 或 "-1" 表示无跳转，由后续动作决定）
    /// </summary>
    public abstract string Process(DialogueAction action);
}
