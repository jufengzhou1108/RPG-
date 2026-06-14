using System.IO;
using UnityEngine;

/// <summary>
/// 对话数据 JSON 文件结构
/// </summary>
[System.Serializable]
public class DialogueDataJson
{
    public string configKey;
}

/// <summary>
/// 对话数据文件读写工具
/// </summary>
public static class DialogueDataIO
{
    private const string DATA_DIR = "TalkData";

    public static DialogueDataJson Load(string dataId)
    {
        string fileName = $"{dataId}.json";
        string persistentPath = Path.Combine(Application.persistentDataPath, DATA_DIR, fileName);
        if (File.Exists(persistentPath))
        {
            return JsonUtility.FromJson<DialogueDataJson>(File.ReadAllText(persistentPath));
        }

        TextAsset textAsset = AddressableManager.Instance.LoadRes<TextAsset>(dataId);
        if (textAsset != null)
        {
            return JsonUtility.FromJson<DialogueDataJson>(textAsset.text);
        }

        Debug.LogError($"DialogueDataIO: 找不到对话数据 [{dataId}]");
        return null;
    }

    public static void Save(string dataId, string newConfigKey)
    {
        var data = new DialogueDataJson { configKey = newConfigKey };
        string json = JsonUtility.ToJson(data, prettyPrint: true);
        string dir = Path.Combine(Application.persistentDataPath, DATA_DIR);
        if (!Directory.Exists(dir))
        {
            Directory.CreateDirectory(dir);
        }
        File.WriteAllText(Path.Combine(dir, $"{dataId}.json"), json);
    }
}

/// <summary>
/// 对话对象 —— 挂载在 Dialogable 层的物体上
/// 通过 dataId 查找 JSON 数据文件获取对话配置 key，支持运行时替换并持久化
/// </summary>
public class DialogueObject : MonoBehaviour
{
    [Tooltip("对话数据 ID（对应 Talk/Data/ 中的 JSON 文件名，不含扩展名）")]
    [SerializeField] private string dataId;

    private DialogueConfig cachedConfig;
    private string cachedConfigKey;

    /// <summary>
    /// 当前对话配置（通过 dataId → JSON → configKey → Addressables 加载）
    /// </summary>
    public DialogueConfig Config
    {
        get
        {
            if (string.IsNullOrEmpty(dataId)) return null;

            DialogueDataJson data = DialogueDataIO.Load(dataId);
            if (data == null) return null;

            string key = data.configKey;
            if (string.IsNullOrEmpty(key))
            {
                Debug.LogWarning($"DialogueObject [{dataId}]: configKey 为空，请在 JSON 文件中填写有效的配置 key");
                return null;
            }

            if (cachedConfig == null || cachedConfigKey != key)
            {
                cachedConfig = AddressableManager.Instance.LoadRes<DialogueConfig>(key);
                cachedConfigKey = key;
            }
            return cachedConfig;
        }
    }

    /// <summary>
    /// 替换对话配置 key 并持久化到 JSON
    /// </summary>
    public void ReplaceConfig(string newKey)
    {
        if (string.IsNullOrEmpty(dataId))
        {
            Debug.LogWarning("DialogueObject: dataId 为空，无法替换配置");
            return;
        }

        DialogueDataIO.Save(dataId, newKey);
        cachedConfig = null;
        cachedConfigKey = null;
    }
}
