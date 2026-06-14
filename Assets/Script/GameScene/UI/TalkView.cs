using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

/// <summary>
/// 对话 UI —— 只显示当前说话人名字 + 打字机效果 + 选项/继续提示
/// 通过 IPointerClickHandler 响应点击：打字中→跳过 / 等继续→推进
///
/// 流程：
///   无选项 → 打字机播完 → 显示继续提示图 → 点击 → onDialogueComplete(0)
///   有选项 → 打字机播完 → 显示按钮等待点击 → onDialogueComplete(1/2/3)
///   打字机播放中点击 → 跳过打字机直接显示全部文字
/// </summary>
public class TalkView : MonoBehaviour, IPointerClickHandler
{
    [Header("名字")]
    [SerializeField] private Text myNameText;
    [SerializeField] private Image myNameImage;
    [SerializeField] private Text otherNameText;
    [SerializeField] private Image otherNameImage;

    [Header("内容文本（TextMeshPro）")]
    [SerializeField] private TextMeshProUGUI contentText;

    [Header("继续提示")]
    [Tooltip("打字机播完后显示的\"点击继续\"提示图（无选项时）")]
    [SerializeField] private Image endTipImage;

    [Header("选项按钮")]
    [SerializeField] private Button optionBtn1;
    [SerializeField] private Button optionBtn2;
    [SerializeField] private Button optionBtn3;

    [Header("打字机设置")]
    [SerializeField] private float charsPerSecond = 30f;

    [Header("事件")]
    [Tooltip("对话完成事件。参数：0=点击继续，1/2/3=玩家选择的选项编号")]
    public UnityEvent<int> onDialogueComplete;

    private Coroutine typewriterCoroutine;
    private bool isPlaying;
    private string[] currentOptions;                         // 当前选项文本数组
    private bool waitingForChoice;                           // 是否正在等待玩家选择选项
    private bool waitingForContinue;                         // 是否正在等待玩家点击继续（无选项时）

    void Awake()
    {

        // 初始隐藏
        HideAllOptions();
        HideEndTip();

        // 玩家点击选项或点击继续 → 通知 TalkManager（参数：0=继续，1/2/3=选项）
        if (onDialogueComplete == null)
        {
            onDialogueComplete = new UnityEvent<int>();
        }
        onDialogueComplete.AddListener(TalkManager.Instance.SelectOption);
    }

    void OnEnable()
    {
        // 订阅对话更新事件
        TalkManager.Instance.OnDialogueUpdate += ApplyDialogueData;

        // 处理时序：UI 可能在 StartDialogue 之后才加载完成，重放当前对话
        TalkManager.Instance.ReplayCurrentDialogue();
    }

    void OnDisable()
    {
        StopTypewriter();
        HideAllOptions();
        HideEndTip();
        waitingForChoice = false;
        waitingForContinue = false;

        // 取消订阅对话更新事件
        TalkManager.Instance.OnDialogueUpdate -= ApplyDialogueData;
    }

    #region 文本修改接口

    /// <summary>
    /// 设置我的名字（传 null 或空字符串则隐藏文本和图片）
    /// </summary>
    public void SetMyName(string name_)
    {
        bool show = !string.IsNullOrEmpty(name_);

        if (myNameText != null)
        {
            myNameText.gameObject.SetActive(show);
            if (show) myNameText.text = name_;
        }

        if (myNameImage != null)
        {
            myNameImage.gameObject.SetActive(show);
        }
    }

    /// <summary>
    /// 设置对方名字（传 null 或空字符串则隐藏文本和图片）
    /// </summary>
    public void SetOtherName(string name_)
    {
        bool show = !string.IsNullOrEmpty(name_);

        if (otherNameText != null)
        {
            otherNameText.gameObject.SetActive(show);
            if (show) otherNameText.text = name_;
        }

        if (otherNameImage != null)
        {
            otherNameImage.gameObject.SetActive(show);
        }
    }

    /// <summary>
    /// 设置对话内容并开始打字机效果
    /// </summary>
    public void SetContent(string content)
    {
        if (contentText == null) return;

        StopTypewriter();
        HideAllOptions();
        HideEndTip();

        contentText.text = content;
        contentText.maxVisibleCharacters = 0;

        typewriterCoroutine = StartCoroutine(TypewriterRoutine(content));
    }

    /// <summary>
    /// 一次性设置对话文本与选项，开始播放
    /// </summary>
    /// <param name="myName">我的名字</param>
    /// <param name="otherName">对方名字</param>
    /// <param name="content">对话内容</param>
    /// <param name="options">选项文本（最多3个，可传 null 表示无选项）</param>
    public void ShowDialogue(string myName, string otherName, string content, string[] options = null)
    {
        currentOptions = options;
        SetMyName(myName);
        SetOtherName(otherName);
        SetContent(content);
    }

    /// <summary>
    /// 通过事件数据刷新对话显示（对接 TalkManager.OnDialogueUpdate）
    /// 只显示当前说话人的名字
    /// </summary>
    public void ApplyDialogueData(DialogueEventData data)
    {
        // 只显示当前说话人：说话人匹配 leftName → 显示在左侧，匹配 rightName → 显示在右侧
        string myName = "";
        string otherName = "";

        if (!string.IsNullOrEmpty(data.speaker))
        {
            if (data.speaker == data.leftName)
            {
                myName = data.leftName;
            }
            else if (data.speaker == data.rightName)
            {
                otherName = data.rightName;
            }
        }

        // 如果对话已结束（dialogueId 为空），隐藏对话 UI
        if (string.IsNullOrEmpty(data.dialogueId))
        {
            gameObject.SetActive(false);
            return;
        }

        gameObject.SetActive(true);
        ShowDialogue(myName, otherName, data.content, data.options);
    }

    /// <summary>
    /// 立刻显示全部内容（跳过打字机效果）
    /// </summary>
    public void SkipToEnd()
    {
        if (contentText == null) return;

        StopTypewriter();
        contentText.maxVisibleCharacters = contentText.text.Length;
        OnContentComplete();
    }

    /// <summary>
    /// 是否正在播放打字机效果
    /// </summary>
    public bool IsPlaying => isPlaying;

    /// <summary>
    /// 是否正在等待玩家点击选项（打字机已播完，选项按钮已显示）
    /// </summary>
    public bool IsWaitingForChoice => waitingForChoice;

    /// <summary>
    /// 是否正在等待玩家点击继续（无选项时，结束提示已显示）
    /// </summary>
    public bool IsWaitingForContinue => waitingForContinue;

    #endregion

    #region 选项按钮

    /// <summary>
    /// 隐藏所有选项按钮
    /// </summary>
    private void HideAllOptions()
    {
        if (optionBtn1 != null) optionBtn1.gameObject.SetActive(false);
        if (optionBtn2 != null) optionBtn2.gameObject.SetActive(false);
        if (optionBtn3 != null) optionBtn3.gameObject.SetActive(false);
    }

    /// <summary>
    /// 设置按钮文字：优先取子节点的 TextMeshProUGUI，找不到再用传统 Text
    /// </summary>
    private void SetButtonText(Button btn, string text)
    {
        if (btn == null) return;

        var tmp = btn.GetComponentInChildren<TextMeshProUGUI>();
        if (tmp != null)
        {
            tmp.text = text;
            return;
        }

        var legacy = btn.GetComponentInChildren<Text>();
        if (legacy != null)
        {
            legacy.text = text;
        }
    }

    /// <summary>
    /// 根据当前选项数量显示按钮（优先低索引），绑定点击事件
    /// </summary>
    private void ShowOptions(string[] options)
    {
        if (options == null || options.Length == 0) return;

        Button[] btns = { optionBtn1, optionBtn2, optionBtn3 };

        for (int i = 0; i < btns.Length; i++)
        {
            if (btns[i] == null) continue;

            if (i < options.Length)
            {
                // 设置按钮文本（优先 TMP，fallback 传统 Text）
                SetButtonText(btns[i], options[i]);

                // 绑定点击事件（先清再绑，避免重复）
                btns[i].onClick.RemoveAllListeners();
                int choice = i + 1; // 选项编号 1/2/3
                btns[i].onClick.AddListener(() => OnOptionClicked(choice));

                btns[i].gameObject.SetActive(true);
            }
            else
            {
                btns[i].gameObject.SetActive(false);
            }
        }

        waitingForChoice = true;
    }

    /// <summary>
    /// 玩家点击选项按钮
    /// </summary>
    private void OnOptionClicked(int choice)
    {
        waitingForChoice = false;
        HideAllOptions();
        onDialogueComplete?.Invoke(choice);
    }

    #endregion

    #region 结束提示与点击继续

    private void HideEndTip()
    {
        waitingForContinue = false;
        if (endTipImage != null) endTipImage.gameObject.SetActive(false);
    }

    private void ShowEndTip()
    {
        waitingForContinue = true;
        if (endTipImage != null) endTipImage.gameObject.SetActive(true);
    }

    /// <summary>
    /// IPointerClickHandler 实现 —— 点击对话面板：
    ///   打字机播放中 → 直接跳到结束
    ///   等待点击继续（结束提示显示中）→ 推进对话
    /// </summary>
    public void OnPointerClick(PointerEventData eventData)
    {
        if (isPlaying)
        {
            SkipToEnd();
            return;
        }

        if (waitingForContinue)
        {
            HideEndTip();
            onDialogueComplete?.Invoke(0);
        }
    }

    #endregion

    #region 打字机效果

    private IEnumerator TypewriterRoutine(string fullText)
    {
        isPlaying = true;
        int totalChars = fullText.Length;
        float interval = 1f / charsPerSecond;

        for (int i = 1; i <= totalChars; i++)
        {
            contentText.maxVisibleCharacters = i;
            yield return new WaitForSeconds(interval);
        }

        isPlaying = false;
        typewriterCoroutine = null;
        OnContentComplete();
    }

    private void StopTypewriter()
    {
        if (typewriterCoroutine != null)
        {
            StopCoroutine(typewriterCoroutine);
            typewriterCoroutine = null;
        }
        isPlaying = false;
    }

    /// <summary>
    /// 内容播放完成：有选项→显示按钮，无选项→显示结束提示等待点击
    /// </summary>
    private void OnContentComplete()
    {
        if (currentOptions != null && currentOptions.Length > 0)
        {
            ShowOptions(currentOptions);
        }
        else
        {
            ShowEndTip();
        }
    }

    #endregion
}
