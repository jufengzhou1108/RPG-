using System.Collections.Generic;
using TMPro;
using UnityEngine;

/// <summary>
/// 任务详情面板 —— 包含一个任务内容文本 + 多个 TaskAimItem
/// 加载时通过 AddressableManager 按需实例化 TaskAimItem 预制体
///
/// 预制体结构：
///   TalkDetails (本脚本)              ← 自身即为 TaskAimItem 的挂载容器
///     ├─ TitleText (TextMeshProUGUI)    ← 任务标题
///     └─ ContentText (TextMeshProUGUI)  ← 任务内容描述
/// （TaskAimItem 运行时动态添加到 TalkDetails.transform 下）
/// </summary>
public class TalkDetails : MonoBehaviour
{
    [Header("任务标题")]
    [SerializeField] private TextMeshProUGUI titleText;           // 任务标题

    [Header("任务内容")]
    [SerializeField] private TextMeshProUGUI contentText;         // 任务内容文本

    [Header("目标项")]
    [SerializeField] private string aimItemPrefabKey = "TaskAimItem"; // TaskAimItem 的 Addressable 键名

    private List<GameObject> loadedAimItems = new List<GameObject>();

    #region 外部接口

    /// <summary>
    /// 显示指定任务 ID 的详情（从 TaskManager 获取数据）
    /// </summary>
    public void Show(string taskId)
    {
        TaskDisplayData? data = TaskManager.Instance.GetTaskDisplayData(taskId);
        if (data == null)
        {
            Debug.LogWarning($"TalkDetails: 找不到任务 [{taskId}]");
            return;
        }
        Show(data.Value);
    }

    /// <summary>
    /// 使用已获取的任务数据显示详情
    /// </summary>
    public void Show(TaskDisplayData data)
    {
        ClearAimItems();
        SetTitle(data.taskTitle);
        SetContent(data.taskContent);

        if (data.items != null)
        {
            foreach (TaskItemDisplayData item in data.items)
            {
                AddAimItem(item);
            }
        }
    }

    /// <summary>
    /// 设置任务标题
    /// </summary>
    public void SetTitle(string title)
    {
        if (titleText != null)
        {
            titleText.text = title;
        }
    }

    /// <summary>
    /// 设置任务内容文本
    /// </summary>
    public void SetContent(string content)
    {
        if (contentText != null)
        {
            contentText.text = content;
        }
    }

    /// <summary>
    /// 清除所有已加载的目标项
    /// </summary>
    public void ClearAimItems()
    {
        foreach (GameObject item in loadedAimItems)
        {
            if (item != null) Destroy(item);
        }
        loadedAimItems.Clear();
    }

    #endregion

    #region 内部

    /// <summary>
    /// 通过 AddressableManager 异步加载一个 TaskAimItem 预制体并填充数据
    /// </summary>
    private void AddAimItem(TaskItemDisplayData itemData)
    {
        AddressableManager.Instance.LoadResAsync<GameObject>(aimItemPrefabKey, (prefab) =>
        {
            if (prefab == null)
            {
                Debug.LogError($"TalkDetails: 加载 TaskAimItem 预制体 [{aimItemPrefabKey}] 失败");
                return;
            }

            GameObject instance = Instantiate(prefab, transform);
            TaskAimItem aimItem = instance.GetComponent<TaskAimItem>();
            if (aimItem != null)
            {
                aimItem.SetData(itemData.itemContent, itemData.completedCount, itemData.targetCount);
            }
            loadedAimItems.Add(instance);
        });
    }

    #endregion

    void OnDestroy()
    {
        ClearAimItems();
    }
}
