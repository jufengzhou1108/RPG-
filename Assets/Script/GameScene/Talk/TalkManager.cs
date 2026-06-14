using System;
using UnityEngine;

/// <summary>
/// 对话管理器（非Mono单例）
/// 驱动对话流程：读取配置 → 推送事件 → 接受选项 → 通过处理器获取下一条对话
/// </summary>
public class TalkManager : Singleton<TalkManager>
{
    private DialogueConfig config;
    private DialogueEntry currentEntry;

    /// <summary>
    /// 对话更新事件 —— 外部（如 TalkUI）订阅此事件来刷新显示
    /// </summary>
    public event Action<DialogueEventData> OnDialogueUpdate;

    #region 配置与名字

    /// <summary>
    /// 设置对话配置（由外部在对话开始前调用）
    /// </summary>
    public void SetConfig(DialogueConfig config)
    {
        this.config = config;
    }

    /// <summary>
    /// 根据对话 ID 查找条目
    /// </summary>
    private DialogueEntry FindEntry(string dialogueId)
    {
        if (config == null || config.dialogues == null) return null;

        foreach (var entry in config.dialogues)
        {
            if (entry != null && entry.dialogueId == dialogueId)
                return entry;
        }
        return null;
    }

    #endregion

    #region 对话驱动

    /// <summary>
    /// 当前是否有活跃的对话
    /// </summary>
    public bool IsDialogueActive => currentEntry != null;

    /// <summary>
    /// 开始一段对话
    /// </summary>
    /// <param name="dialogueId">起始对话 ID</param>
    public void StartDialogue(string dialogueId)
    {
        EventCenter.Instance.EventTrigger(new DialogueStartEvent());
        ShowDialogue(dialogueId);
    }

    /// <summary>
    /// 重放当前对话的更新事件（供 TalkUI 在 OnEnable 中调用，处理 UI 异步加载的时序问题）
    /// </summary>
    public void ReplayCurrentDialogue()
    {
        if (currentEntry == null) return;

        string[] optionTexts = null;
        if (currentEntry.options != null && currentEntry.options.Length > 0)
        {
            optionTexts = Array.ConvertAll(currentEntry.options, o => o?.optionText);
        }

        OnDialogueUpdate?.Invoke(new DialogueEventData
        {
            dialogueId = currentEntry.dialogueId,
            content   = currentEntry.content,
            speaker   = currentEntry.speaker,
            leftName  = currentEntry.leftName,
            rightName = currentEntry.rightName,
            options   = optionTexts
        });
    }

    /// <summary>
    /// 玩家选择选项后的回调（由外部在 TalkUI.onDialogueComplete 中调用）
    /// </summary>
    /// <param name="optionIndex">0=无选项/继续，1/2/3=选项编号</param>
    public void SelectOption(int optionIndex)
    {
        if (currentEntry == null) return;

        // 1. 收集动作列表
        DialogueAction[] actions = null;

        if (optionIndex >= 1 && optionIndex <= 3)
        {
            int idx = optionIndex - 1;
            if (currentEntry.options != null && idx < currentEntry.options.Length)
            {
                actions = currentEntry.options[idx].actions;
            }
        }
        else
        {
            actions = currentEntry.actions;
        }

        // 2. 按顺序执行所有动作，取最后一个有效跳转
        string nextId = null;
        if (actions != null)
        {
            foreach (DialogueAction action in actions)
            {
                if (action == null || string.IsNullOrEmpty(action.type)) continue;

                DialogueHandlerBase handler = DialogueHandlerBase.GetHandler(action.type);
                string result;
                if (handler != null)
                {
                    result = handler.Process(action);
                }
                else
                {
                    // 无对应处理器 → param 直接作为跳转 ID
                    result = action.param;
                }

                if (!string.IsNullOrEmpty(result) && result != "-1")
                {
                    nextId = result;
                }
            }
        }

        // 3. 无跳转 → 对话结束
        if (string.IsNullOrEmpty(nextId) || nextId == "-1")
        {
            EventCenter.Instance.EventTrigger(new DialogueEndEvent());
            currentEntry = null;
            OnDialogueUpdate?.Invoke(new DialogueEventData { dialogueId = null });
            ViewManager.Instance.Hide<TalkView>();
            return;
        }

        ShowDialogue(nextId);
    }

    /// <summary>
    /// 显示指定 ID 的对话并推送事件
    /// </summary>
    private void ShowDialogue(string dialogueId)
    {
        if (config == null)
        {
            UnityEngine.Debug.LogError("TalkManager: 未设置 DialogueConfig，请先调用 SetConfig()");
            return;
        }

        DialogueEntry entry = FindEntry(dialogueId);
        if (entry == null)
        {
            UnityEngine.Debug.LogError($"TalkManager: 找不到对话 ID [{dialogueId}]");
            return;
        }

        currentEntry = entry;

        // 构建选项文本数组
        string[] optionTexts = null;
        if (entry.options != null && entry.options.Length > 0)
        {
            optionTexts = Array.ConvertAll(entry.options, o => o?.optionText);
        }

        // 构建事件数据
        DialogueEventData data = new DialogueEventData
        {
            dialogueId = entry.dialogueId,
            content   = entry.content,
            speaker   = entry.speaker,
            leftName  = entry.leftName,
            rightName = entry.rightName,
            options   = optionTexts
        };

        OnDialogueUpdate?.Invoke(data);
    }

    #endregion
}
