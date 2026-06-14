using System;
using UnityEngine;

/// <summary>
/// 任务项配置 —— 一个任务包含多个任务项（目标）
/// 每个任务项的完成进度可平均量化（如和3个人聊天，每人进度+1）
/// </summary>
[Serializable]
public class TaskItemEntry
{
    [Tooltip("任务项ID（唯一标识）")]
    public string itemId;

    [Tooltip("任务项描述（目标内容，如'和村民聊天'）")]
    public string itemContent;

    [Tooltip("目标数量（任务项需要的完成次数）")]
    public int targetCount = 1;
}

/// <summary>
/// 任务配置文件（ScriptableObject）—— 一个文件 = 一个任务，文件名 = 任务ID
/// 在 Project 窗口中右键 → Create → RPG → TaskConfig 创建
/// 放入 Assets/.../Task/Config/Resources/ 文件夹，
/// TaskManager 通过 Resources.Load 按任务 ID 按需加载
/// </summary>
[CreateAssetMenu(fileName = "TaskConfig", menuName = "RPG/TaskConfig", order = 4)]
public class TaskConfig : ScriptableObject
{
    [Header("基本信息")]
    [Tooltip("任务ID（与文件名一致）")]
    public string taskId;

    [Tooltip("任务标题")]
    public string taskTitle;

    [Tooltip("任务内容描述")]
    public string taskContent;

    [Header("任务项")]
    [Tooltip("任务项列表（要完成的目标）")]
    public TaskItemEntry[] taskItems;
}
