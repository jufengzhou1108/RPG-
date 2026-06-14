/// <summary>
/// 任务项显示数据 —— 将配置与玩家进度合并，供 UI 使用
/// </summary>
public struct TaskItemDisplayData
{
    /// <summary>任务项ID</summary>
    public string itemId;

    /// <summary>任务项描述（目标内容）</summary>
    public string itemContent;

    /// <summary>目标数量</summary>
    public int targetCount;

    /// <summary>已完成数量</summary>
    public int completedCount;

    /// <summary>该任务项是否已完成</summary>
    public bool IsCompleted => completedCount >= targetCount;
}

/// <summary>
/// 任务显示数据 —— 将任务配置与玩家进度合并，供 UI 使用
/// TaskManager.GetTaskDisplayData(taskId) 返回此结构
/// </summary>
public struct TaskDisplayData
{
    /// <summary>任务ID</summary>
    public string taskId;

    /// <summary>任务标题</summary>
    public string taskTitle;

    /// <summary>任务内容描述</summary>
    public string taskContent;

    /// <summary>任务项显示数据列表（配置 + 进度）</summary>
    public TaskItemDisplayData[] items;
}
