/// <summary>
/// 对话开始事件（EventCenter 用，无参数）
/// </summary>
public struct DialogueStartEvent { }

/// <summary>
/// 对话结束事件（EventCenter 用，无参数）
/// </summary>
public struct DialogueEndEvent { }

/// <summary>
/// 对话事件数据 —— TalkManager 通过事件推送给外部（TalkUI）用于刷新显示
/// </summary>
public struct DialogueEventData
{
    /// <summary>
    /// 当前对话 ID
    /// </summary>
    public string dialogueId;

    /// <summary>
    /// 对话内容
    /// </summary>
    public string content;

    /// <summary>
    /// 当前说话人
    /// </summary>
    public string speaker;

    /// <summary>
    /// 左侧名字
    /// </summary>
    public string leftName;

    /// <summary>
    /// 右侧名字
    /// </summary>
    public string rightName;

    /// <summary>
    /// 选项文本（最多3个，无选项时为 null 或空数组）
    /// </summary>
    public string[] options;
}
