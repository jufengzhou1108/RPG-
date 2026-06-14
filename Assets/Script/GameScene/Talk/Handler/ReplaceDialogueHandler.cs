using UnityEngine;

/// <summary>
/// 替换对话处理器 —— 替换指定对象的对话配置
///
/// param 格式："{对象名} {配置key}"（空格分隔）
///   对象名 → GameObject.Find 查找目标
///   配置key → Addressables 加载新的 DialogueConfig
///
/// 示例 param: "NPC_Villager VillagerQuestDone"
///   找到名为 NPC_Villager 的对象，将其 DialogueObject 的配置替换为 VillagerQuestDone
/// </summary>
public class ReplaceDialogueHandler : DialogueHandlerBase
{
    [RuntimeInitializeOnLoadMethod]
    static void Init()
    {
        Register("替换对话", new ReplaceDialogueHandler());
    }

    public override string Process(DialogueAction action)
    {
        if (string.IsNullOrEmpty(action.param)) return null;

        // 按空格拆分为对象名和配置 key
        string[] parts = action.param.Split(' ');
        if (parts.Length < 2)
        {
            Debug.LogWarning("ReplaceDialogueHandler: param 格式应为 \"对象名 配置key\"（空格分隔）");
            return null;
        }

        string objName = parts[0];
        string configKey = parts[1];

        // 查找目标对象
        GameObject target = GameObject.Find(objName);
        if (target == null)
        {
            Debug.LogWarning($"ReplaceDialogueHandler: 找不到对象 [{objName}]");
            return null;
        }

        // 获取 DialogueObject 组件
        DialogueObject dialogueObj = target.GetComponent<DialogueObject>();
        if (dialogueObj == null)
        {
            Debug.LogWarning($"ReplaceDialogueHandler: 对象 [{objName}] 上没有 DialogueObject 组件");
            return null;
        }

        // 替换配置 key（持久化到 DialogueData 文件）
        dialogueObj.ReplaceConfig(configKey);
        return null;
    }
}
