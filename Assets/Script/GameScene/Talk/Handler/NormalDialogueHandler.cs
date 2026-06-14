using UnityEngine;

/// <summary>
/// 普通对话处理器 —— 直接返回 param 作为下一条对话 ID
/// param == "-1" 或空 → 返回 null（对话不跳转，由后续动作决定）
/// </summary>
public class NormalDialogueHandler : DialogueHandlerBase
{
    [RuntimeInitializeOnLoadMethod]
    static void Init()
    {
        Register("普通对话", new NormalDialogueHandler());
    }

    public override string Process(DialogueAction action)
    {
        if (string.IsNullOrEmpty(action.param) || action.param == "-1")
            return null;

        return action.param;
    }
}
