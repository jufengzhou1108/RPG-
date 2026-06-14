using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

/// <summary>
/// 任务进度记录（JSON 序列化用）—— 单个任务项的完成进度
/// </summary>
[Serializable]
public class TaskItemProgress
{
    public string taskId;
    public string itemId;
    public int completedCount;
}

/// <summary>
/// 任务进度存档（JSON 根对象）—— 所有任务项进度的扁平列表
/// </summary>
[Serializable]
public class TaskSaveData
{
    public List<TaskItemProgress> items = new List<TaskItemProgress>();
}

/// <summary>
/// 任务管理器（非Mono单例）
///
/// 任务 ID 由数据文件驱动：玩家接受任务时写入进度存档，
/// 列表仅显示 progressDict 中已有的任务（即玩家接受过的任务）。
///
/// 配置文件位于 Task/Config/，每个任务一个 .asset，通过 Addressables 按需加载
/// </summary>
public class TaskManager : Singleton<TaskManager>
{
    private const string SAVE_NAME = "progress";                    // JsonTool 存档名

    private TaskSaveData saveData;                                  // 进度存档
    private Dictionary<string, Dictionary<string, int>> progressDict = new Dictionary<string, Dictionary<string, int>>();
    private Dictionary<string, TaskConfig> configCache = new Dictionary<string, TaskConfig>();

    /// <summary>
    /// 任务数据变更事件 —— 外部（如任务列表 UI）订阅此事件来刷新显示
    /// </summary>
    public event Action OnTaskDataChanged;

    #region 初始化

    /// <summary>
    /// 初始化：加载进度存档 → 构建查询字典
    /// 由 GameSceneManager 在场景启动时调用
    /// </summary>
    public void Init()
    {
        configCache = new Dictionary<string, TaskConfig>();

        // 1. 加载进度存档（JSON ← JsonTool）
        LoadTaskData();

        // 2. 构建进度查询字典
        BuildProgressDict();
    }

    #endregion

    #region 配置加载

    /// <summary>
    /// 按需加载指定任务配置（先从缓存查，未命中则通过 AddressableManager 同步加载）
    /// .asset 文件名 = 任务ID = Addressable 键名
    /// </summary>
    private TaskConfig LoadConfig(string taskId)
    {
        if (string.IsNullOrEmpty(taskId)) return null;

        if (configCache.TryGetValue(taskId, out TaskConfig cached))
        {
            return cached;
        }

        TaskConfig cfg = AddressableManager.Instance.LoadRes<TaskConfig>(taskId);
        if (cfg != null)
        {
            configCache[taskId] = cfg;
        }
        return cfg;
    }

    #endregion

    #region 数据查询

    /// <summary>
    /// 获取所有未完成任务的 ID 列表
    /// 仅返回玩家已接受的任务（即 progressDict 中存在的任务）
    /// </summary>
    public string[] GetUnfinishedTaskIds()
    {
        List<string> unfinished = new List<string>();
        foreach (string taskId in progressDict.Keys)
        {
            if (string.IsNullOrEmpty(taskId)) continue;

            TaskConfig cfg = LoadConfig(taskId);
            if (cfg != null && IsTaskCompleted(cfg)) continue;

            unfinished.Add(taskId);
        }

        return unfinished.ToArray();
    }

    /// <summary>
    /// 根据任务 ID 返回显示所需的合并数据（配置 + 进度）
    /// 配置按需通过 AddressableManager 加载
    /// </summary>
    /// <param name="taskId">任务 ID（Addressable 键名）</param>
    /// <returns>任务显示数据，未找到返回 null</returns>
    public TaskDisplayData? GetTaskDisplayData(string taskId)
    {
        TaskConfig cfg = LoadConfig(taskId);
        if (cfg == null) return null;

        return new TaskDisplayData
        {
            taskId      = cfg.taskId,
            taskTitle   = cfg.taskTitle,
            taskContent = cfg.taskContent,
            items       = BuildItemDisplayData(cfg)
        };
    }

    /// <summary>
    /// 获取指定任务项的完成数量
    /// </summary>
    public int GetItemProgress(string taskId, string itemId)
    {
        if (progressDict.TryGetValue(taskId, out var itemDict)
            && itemDict.TryGetValue(itemId, out int count))
        {
            return count;
        }
        return 0;
    }

    #endregion

    #region 数据修改

    /// <summary>
    /// 接受任务 —— 将该任务加入进度存档（所有任务项初始进度为 0）
    /// 已接受的任务不会重复初始化
    /// </summary>
    /// <param name="taskId">任务 ID（Addressable 键名）</param>
    public void AcceptTask(string taskId)
    {
        // 已接受则跳过
        if (progressDict.ContainsKey(taskId)) return;

        // 按需加载配置，初始化所有任务项进度为 0
        TaskConfig cfg = LoadConfig(taskId);
        if (cfg == null)
        {
            Debug.LogError($"TaskManager.AcceptTask: 找不到任务配置 [{taskId}]");
            return;
        }

        progressDict[taskId] = new Dictionary<string, int>();
        if (cfg.taskItems != null)
        {
            foreach (TaskItemEntry item in cfg.taskItems)
            {
                if (item != null && !string.IsNullOrEmpty(item.itemId))
                {
                    progressDict[taskId][item.itemId] = 0;
                }
            }
        }

        SyncToSaveData();
        SaveTaskData();
        OnTaskDataChanged?.Invoke();
    }

    /// <summary>
    /// 增加任务项进度
    /// </summary>
    public void AddProgress(string taskId, string itemId, int count = 1)
    {
        if (count <= 0) return;

        // 首次接触该任务 → 先走接受流程，初始化所有任务项为 0
        if (!progressDict.ContainsKey(taskId))
        {
            AcceptTask(taskId);
            if (!progressDict.ContainsKey(taskId)) return; // 接受失败（配置不存在）
        }

        if (!progressDict[taskId].ContainsKey(itemId))
            progressDict[taskId][itemId] = 0;

        // 按需加载配置，验证不超上限
        TaskConfig cfg = LoadConfig(taskId);
        if (cfg != null)
        {
            TaskItemEntry itemEntry = FindTaskItemEntry(cfg, itemId);
            if (itemEntry != null)
            {
                int current = progressDict[taskId][itemId];
                if (current >= itemEntry.targetCount) return;
                count = Mathf.Min(count, itemEntry.targetCount - current);
            }
        }

        progressDict[taskId][itemId] += count;

        SyncToSaveData();
        SaveTaskData();
        OnTaskDataChanged?.Invoke();
    }

    /// <summary>
    /// 重置指定任务的所有进度
    /// </summary>
    public void ResetTaskProgress(string taskId)
    {
        if (!progressDict.ContainsKey(taskId)) return;

        progressDict.Remove(taskId);
        SyncToSaveData();
        SaveTaskData();
        OnTaskDataChanged?.Invoke();
    }

    /// <summary>
    /// 清除所有任务进度
    /// </summary>
    public void ClearAllProgress()
    {
        progressDict.Clear();
        saveData = new TaskSaveData();
        SaveTaskData();
        OnTaskDataChanged?.Invoke();
    }

    #endregion

    #region 内部方法

    private bool IsTaskCompleted(TaskConfig cfg)
    {
        if (cfg.taskItems == null || cfg.taskItems.Length == 0) return true;

        foreach (TaskItemEntry item in cfg.taskItems)
        {
            if (item == null || string.IsNullOrEmpty(item.itemId)) continue;
            if (GetItemProgress(cfg.taskId, item.itemId) < item.targetCount)
                return false;
        }
        return true;
    }

    private TaskItemEntry FindTaskItemEntry(TaskConfig cfg, string itemId)
    {
        if (cfg.taskItems == null) return null;
        foreach (TaskItemEntry item in cfg.taskItems)
        {
            if (item != null && item.itemId == itemId) return item;
        }
        return null;
    }

    private TaskItemDisplayData[] BuildItemDisplayData(TaskConfig cfg)
    {
        if (cfg.taskItems == null || cfg.taskItems.Length == 0)
            return Array.Empty<TaskItemDisplayData>();

        TaskItemDisplayData[] items = new TaskItemDisplayData[cfg.taskItems.Length];
        for (int i = 0; i < cfg.taskItems.Length; i++)
        {
            TaskItemEntry entry = cfg.taskItems[i];
            if (entry == null) continue;

            items[i] = new TaskItemDisplayData
            {
                itemId         = entry.itemId,
                itemContent    = entry.itemContent,
                targetCount    = entry.targetCount,
                completedCount = GetItemProgress(cfg.taskId, entry.itemId)
            };
        }
        return items;
    }

    #endregion

    #region JSON 持久化

    private void LoadTaskData()
    {
        string dir = GetSaveDirectory();
        saveData = JsonTool.LoadJson<TaskSaveData>(SAVE_NAME, dir);
    }

    private void SaveTaskData()
    {
        string dir = GetSaveDirectory();
        if (!Directory.Exists(dir))
        {
            Directory.CreateDirectory(dir);
        }

        JsonTool.SaveData(saveData, SAVE_NAME, dir);
    }

    private void SyncToSaveData()
    {
        saveData = new TaskSaveData();
        foreach (var taskKvp in progressDict)
        {
            foreach (var itemKvp in taskKvp.Value)
            {
                saveData.items.Add(new TaskItemProgress
                {
                    taskId         = taskKvp.Key,
                    itemId         = itemKvp.Key,
                    completedCount = itemKvp.Value
                });
            }
        }
    }

    private void BuildProgressDict()
    {
        progressDict = new Dictionary<string, Dictionary<string, int>>();
        if (saveData?.items == null) return;

        foreach (TaskItemProgress item in saveData.items)
        {
            if (item == null || string.IsNullOrEmpty(item.taskId)) continue;

            if (!progressDict.ContainsKey(item.taskId))
                progressDict[item.taskId] = new Dictionary<string, int>();

            progressDict[item.taskId][item.itemId ?? string.Empty] = item.completedCount;
        }
    }

    private string GetSaveDirectory()
    {
#if UNITY_EDITOR
        // 编辑器下写项目内 Task/Data/，方便直接查看调试
        // 末尾加 /，兼容 JsonTool 直接用字符串拼接路径的方式
        return Path.Combine(Application.dataPath, "Script/GameScene/Task/Data") + "/";
#else
        return Path.Combine(Application.persistentDataPath, "TaskData") + "/";
#endif
    }

    #endregion
}
