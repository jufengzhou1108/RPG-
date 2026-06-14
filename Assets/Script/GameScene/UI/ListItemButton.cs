using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

/// <summary>
/// 列表项按钮 —— 提供按钮点击事件订阅接口、文本修改接口和文本组件访问
/// </summary>
[RequireComponent(typeof(Button))]
public class ListItemButton : MonoBehaviour
{
    [Header("文本组件")]
    [SerializeField] private TextMeshProUGUI labelText;   // 子节点的 TMP 文本（优先使用）
    [SerializeField] private Text labelTextLegacy;         // 子节点的传统 Text（备用）

    private Button button;

    void Awake()
    {
        button = GetComponent<Button>();

        // 未手动拖入则自动查找子节点中的文本组件（优先 TMP）
        if (labelText == null && labelTextLegacy == null)
        {
            labelText = GetComponentInChildren<TextMeshProUGUI>();
            if (labelText == null)
            {
                labelTextLegacy = GetComponentInChildren<Text>();
            }
        }
    }

    #region 按钮事件订阅

    /// <summary>
    /// 订阅按钮点击事件
    /// </summary>
    public void AddClickListener(UnityAction action)
    {
        if (button != null)
        {
            button.onClick.AddListener(action);
        }
    }

    /// <summary>
    /// 取消订阅按钮点击事件
    /// </summary>
    public void RemoveClickListener(UnityAction action)
    {
        if (button != null)
        {
            button.onClick.RemoveListener(action);
        }
    }

    /// <summary>
    /// 清除所有点击事件监听
    /// </summary>
    public void RemoveAllClickListeners()
    {
        if (button != null)
        {
            button.onClick.RemoveAllListeners();
        }
    }

    /// <summary>
    /// Button 组件的 onClick 事件（供外部直接操作）
    /// </summary>
    public UnityEvent OnClick => button?.onClick;

    #endregion

    #region 文本修改接口

    /// <summary>
    /// 设置列表项显示文本
    /// </summary>
    public void SetText(string text)
    {
        if (labelText != null)
        {
            labelText.text = text;
        }
        else if (labelTextLegacy != null)
        {
            labelTextLegacy.text = text;
        }
    }

    /// <summary>
    /// 获取当前显示文本
    /// </summary>
    public string GetText()
    {
        if (labelText != null)
        {
            return labelText.text;
        }
        if (labelTextLegacy != null)
        {
            return labelTextLegacy.text;
        }
        return string.Empty;
    }

    #endregion

    #region 文本组件获取

    /// <summary>
    /// 获取 TMP 文本组件（优先）
    /// </summary>
    public TextMeshProUGUI TextMesh => labelText;

    /// <summary>
    /// 获取传统 Text 组件（备用）
    /// </summary>
    public Text TextLegacy => labelTextLegacy;

    /// <summary>
    /// 获取 Button 组件
    /// </summary>
    public Button Button => button;

    #endregion
}
