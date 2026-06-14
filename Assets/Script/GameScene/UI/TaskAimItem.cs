using TMPro;
using UnityEngine;

/// <summary>
/// 任务目标项 —— 挂载在 TaskAimItem 预制体上
/// 为外部提供设置目标描述文本和进度文本的接口
///
/// 预制体结构：
///   TaskAimItem (本脚本 + RectTransform)
///     ├─ TargetText (TextMeshProUGUI)  ← 目标描述，如"击败史莱姆"
///     └─ ProgressText (TextMeshProUGUI) ← 进度文本，如"2/5"
/// </summary>
public class TaskAimItem : MonoBehaviour
{
    [Header("文本组件")]
    [SerializeField] private TextMeshProUGUI targetText;      // 目标描述文本
    [SerializeField] private TextMeshProUGUI progressText;    // 进度文本

    void Awake()
    {
        // 未手动拖入则自动从子节点查找（名称匹配）
        if (targetText == null)
        {
            Transform t = transform.Find("TargetText");
            if (t != null) targetText = t.GetComponent<TextMeshProUGUI>();
        }

        if (progressText == null)
        {
            Transform t = transform.Find("ProgressText");
            if (t != null) progressText = t.GetComponent<TextMeshProUGUI>();
        }
    }

    #region 外部接口

    /// <summary>
    /// 设置目标描述文本（如"击败史莱姆"）
    /// </summary>
    public void SetTarget(string text)
    {
        if (targetText != null)
        {
            targetText.text = text;
        }
    }

    /// <summary>
    /// 设置进度文本（如 "2/5"）
    /// </summary>
    public void SetProgress(int completed, int total)
    {
        if (progressText != null)
        {
            progressText.text = $"{completed}/{total}";
        }
    }

    /// <summary>
    /// 设置进度文本（自由格式）
    /// </summary>
    public void SetProgress(string text)
    {
        if (progressText != null)
        {
            progressText.text = text;
        }
    }

    /// <summary>
    /// 一次性设置目标 + 进度
    /// </summary>
    public void SetData(string target, int completed, int total)
    {
        SetTarget(target);
        SetProgress(completed, total);
    }

    #endregion
}
