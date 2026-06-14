using System;
using UnityEngine;

/// <summary>
/// 对话动作 —— 类型 + 参数对，一条对话/选项可包含多个动作，按顺序执行
/// </summary>
[Serializable]
public class DialogueAction
{
    [Tooltip("动作类型（对应 DialogueHandlerBase 注册的 ID）")]
    public string type;

    [Tooltip("动作参数（如任务 ID、下一个对话 ID、-1 表示结束等）")]
    public string param;
}

/// <summary>
/// 单个选项的配置
/// </summary>
[Serializable]
public class OptionEntry
{
    [Tooltip("选项显示文本")]
    public string optionText;

    [Tooltip("选项动作列表（按顺序执行）")]
    public DialogueAction[] actions;
}

/// <summary>
/// 单条对话的配置
/// </summary>
[Serializable]
public class DialogueEntry
{
    [Tooltip("对话ID（唯一标识）")]
    public string dialogueId;

    [Tooltip("对话内容")]
    public string content;

    [Tooltip("说话人")]
    public string speaker;

    [Tooltip("左侧名字")]
    public string leftName;

    [Tooltip("右侧名字")]
    public string rightName;

    [Tooltip("对话自然推进时的动作列表（按顺序执行）")]
    public DialogueAction[] actions;

    [Tooltip("选项列表")]
    public OptionEntry[] options;
}

/// <summary>
/// 对话配置文件（ScriptableObject）
/// 在 Project 窗口中右键 → Create → Game → Dialogue Config 创建
/// </summary>
[CreateAssetMenu(fileName = "NewDialogueConfig", menuName = "Game/Dialogue Config")]
public class DialogueConfig : ScriptableObject
{
    [Tooltip("对话条目列表")]
    public DialogueEntry[] dialogues;

#if UNITY_EDITOR
    private void OnValidate()
    {
        if (dialogues == null) return;

        foreach (var entry in dialogues)
        {
            if (entry?.options != null && entry.options.Length > 3)
            {
                var clamped = new OptionEntry[3];
                Array.Copy(entry.options, clamped, 3);
                entry.options = clamped;
                Debug.LogWarning($"对话 [{entry.content}] 的选项超过3个，已自动截断为3个。");
            }
        }
    }
#endif
}
