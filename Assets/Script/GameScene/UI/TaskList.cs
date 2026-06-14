using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 虚拟化任务列表 —— 管理 ScrollRect 的 Content 尺寸和 ListItemButton 的显示/回收
///
/// 通过 TaskManager 获取未完成任务 ID 列表，计算 Content 总高度，
/// 根据 Content 的 Y 偏移 + 列表项高度 + Viewport 高度计算可见范围，
/// 可见项从对象池取出并定位，不可见项回收到对象池。
///
/// 预制体结构：
///   TaskList (本脚本)
///     └─ ScrollView
///          ├─ Viewport (Mask)
///          │    └─ Content (本脚本通过引用获取)
///          └─ Scrollbar
/// </summary>
public class TaskList : MonoBehaviour
{
    [Header("滚动视图")]
    [SerializeField] private ScrollRect scrollRect;               // ScrollRect 组件
    [SerializeField] private RectTransform content;               // Content RectTransform
    [SerializeField] private RectTransform viewport;              // Viewport RectTransform（用于取高度）

    [Header("列表项参数")]
    [SerializeField] private string itemPrefabKey = "ListItemButton"; // ObjectPool / Addressable 键名
    [SerializeField] private float spacing = 0f;                  // 列表项之间的间距
    [SerializeField] private int poolCeiling = 20;                // 对象池上限

    [Header("事件")]
    public TaskItemClickedEvent onTaskClicked = new TaskItemClickedEvent(); // 列表项点击事件

    private string[] taskIds;                                     // 当前未完成任务的 ID 数组
    private Dictionary<int, GameObject> activeItems = new Dictionary<int, GameObject>();
    private float itemHeight;                                     // 列表项高度（从预制体 RectTransform 读取）
    private float effectiveItemHeight;                            // itemHeight + spacing

    void Awake()
    {
        // 从预制体读取列表项高度（Awake 阶段完成，确保 OnEnable→Refresh 可用）
        LoadItemHeight();
        effectiveItemHeight = itemHeight + spacing;
    }

    void Start()
    {
        if (scrollRect != null)
        {
            scrollRect.onValueChanged.AddListener(OnScroll);
        }

        TaskManager.Instance.OnTaskDataChanged += Refresh;
    }

    /// <summary>
    /// 通过 AddressableManager 加载预制体，取其 RectTransform.rect.height 作为列表项高度
    /// </summary>
    private void LoadItemHeight()
    {
        GameObject prefab = AddressableManager.Instance.LoadRes<GameObject>(itemPrefabKey);
        if (prefab != null)
        {
            RectTransform prefabRect = prefab.GetComponent<RectTransform>();
            itemHeight = prefabRect != null ? prefabRect.rect.height : 60f;
        }
        else
        {
            itemHeight = 60f;
            Debug.LogWarning($"TaskList: 无法加载 [{itemPrefabKey}] 获取高度，使用默认值 60");
        }
    }

    void OnEnable()
    {
        // 每次激活时刷新（可能从其它界面切回来）
        Refresh();
    }

    #region 数据刷新

    /// <summary>
    /// 从 TaskManager 拉取未完成任务列表，重新计算 Content 尺寸，刷新可见项
    /// </summary>
    public void Refresh()
    {
        taskIds = TaskManager.Instance.GetUnfinishedTaskIds();

        // 计算 Content 总高度
        float totalHeight = taskIds.Length * effectiveItemHeight;
        if (content != null)
        {
            content.sizeDelta = new Vector2(content.sizeDelta.x, totalHeight);
        }

        // 重置滚动位置到顶部
        if (content != null)
        {
            content.anchoredPosition = new Vector2(content.anchoredPosition.x, 0);
        }

        // 回收所有当前显示的项
        RecycleAll();

        // 重新计算并显示可见项
        UpdateVisibleItems();
    }

    #endregion

    #region 滚动与可见范围计算

    private void OnScroll(Vector2 _)
    {
        UpdateVisibleItems();
    }

    /// <summary>
    /// 根据当前 Content 的 Y 偏移和 Viewport 高度，计算应显示的索引范围
    /// 不在范围内的项回收到对象池，新进入范围的项从对象池取出
    /// </summary>
    private void UpdateVisibleItems()
    {
        if (taskIds == null || taskIds.Length == 0 || viewport == null || content == null) return;

        float contentY = content.anchoredPosition.y;                    // Content 顶部偏移（向下滚为正值）
        float viewportHeight = viewport.rect.height;

        int newFirst = Mathf.Max(0, Mathf.FloorToInt(contentY / effectiveItemHeight));
        int visibleCount = Mathf.CeilToInt(viewportHeight / effectiveItemHeight) + 1; // +1 缓冲
        int newLast = Mathf.Min(newFirst + visibleCount - 1, taskIds.Length - 1);

        if (newFirst < 0 || newLast < 0) return;

        // 范围没变则跳过
        if (newFirst == firstVisibleIndex && newLast == lastVisibleIndex) return;

        // 回收已移出可见范围的项
        List<int> toRemove = new List<int>();
        foreach (int index in activeItems.Keys)
        {
            if (index < newFirst || index > newLast)
                toRemove.Add(index);
        }
        foreach (int index in toRemove)
        {
            RecycleItem(index);
        }

        // 显示新进入可见范围的项
        for (int i = newFirst; i <= newLast; i++)
        {
            if (!activeItems.ContainsKey(i))
            {
                ShowItem(i);
            }
        }

        firstVisibleIndex = newFirst;
        lastVisibleIndex = newLast;
    }

    private int firstVisibleIndex = -1;
    private int lastVisibleIndex  = -1;

    #endregion

    #region 列表项的显示与回收

    /// <summary>
    /// 从对象池取出一个列表项，定位到指定索引位置，填充数据并绑定点击事件
    /// </summary>
    private void ShowItem(int index)
    {
        if (index < 0 || index >= taskIds.Length) return;

        // 从对象池取出
        GameObject item = ObjectPool.Instance.Pop(itemPrefabKey, poolCeiling);
        if (item == null)
        {
            Debug.LogError($"TaskList: 从对象池取出 [{itemPrefabKey}] 失败");
            return;
        }

        // 设置为 Content 的子节点
        RectTransform rect = item.GetComponent<RectTransform>();
        if (rect == null)
        {
            rect = item.AddComponent<RectTransform>();
        }

        // 重置缩放再挂载，避免坐标系切换导致缩放累积
        rect.localScale = Vector3.one;
        rect.SetParent(content, false);

        // 锚定到 Content 顶部，宽填满，重置高度为预制体原始高度
        rect.anchorMin = new Vector2(0, 1);
        rect.anchorMax = new Vector2(1, 1);
        rect.pivot     = new Vector2(0.5f, 1);
        rect.sizeDelta = new Vector2(0, itemHeight);

        // 忽略父节点的 LayoutGroup，避免手动设置的高度被覆盖
        LayoutElement layout = item.GetComponent<LayoutElement>();
        if (layout == null) layout = item.AddComponent<LayoutElement>();
        layout.ignoreLayout = true;

        // 定位：从上到下排列
        rect.anchoredPosition = new Vector2(0, -index * effectiveItemHeight);

        // 填充文本
        ListItemButton button = item.GetComponent<ListItemButton>();
        if (button != null)
        {
            button.RemoveAllClickListeners();

            TaskDisplayData? data = TaskManager.Instance.GetTaskDisplayData(taskIds[index]);
            if (data != null)
            {
                button.SetText(data.Value.taskTitle);

                // 点击时触发外部事件
                string taskId = taskIds[index];
                button.AddClickListener(() => onTaskClicked.Invoke(taskId));
            }
        }

        activeItems[index] = item;
    }

    /// <summary>
    /// 回收指定索引的列表项到对象池
    /// </summary>
    private void RecycleItem(int index)
    {
        if (activeItems.TryGetValue(index, out GameObject item))
        {
            // 清除监听再入池
            ListItemButton button = item.GetComponent<ListItemButton>();
            if (button != null)
            {
                button.RemoveAllClickListeners();
            }

            // 脱离父节点 + 重置缩放，避免父节点销毁连带销毁，防止坐标系切换缩放累积
            item.transform.SetParent(null);
            item.transform.localScale = Vector3.one;

            ObjectPool.Instance.Push(itemPrefabKey, item, poolCeiling);
            activeItems.Remove(index);
        }
    }

    /// <summary>
    /// 回收所有显示的列表项
    /// </summary>
    private void RecycleAll()
    {
        foreach (int index in new List<int>(activeItems.Keys))
        {
            RecycleItem(index);
        }
        firstVisibleIndex = -1;
        lastVisibleIndex  = -1;
    }

    #endregion

    void OnDestroy()
    {
        if (scrollRect != null)
        {
            scrollRect.onValueChanged.RemoveListener(OnScroll);
        }

        TaskManager.Instance.OnTaskDataChanged -= Refresh;
        RecycleAll();
    }
}

/// <summary>
/// 任务列表项点击事件 —— 参数为被点击的任务 ID
/// </summary>
[Serializable]
public class TaskItemClickedEvent : UnityEngine.Events.UnityEvent<string> { }
