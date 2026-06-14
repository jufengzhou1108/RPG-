using System.IO;
using UnityEditor;
using UnityEngine;

/// <summary>
/// 右键菜单 → Create → RPG → Dialogue Data JSON
/// 在选中目录创建对话数据 JSON 文件
/// </summary>
public static class DialogueDataCreator
{
    private const string TEMPLATE = @"{
  ""configKey"": """"
}";

    [MenuItem("Assets/Create/Dialogue Data JSON", false, 50)]
    private static void Create()
    {
        string dir = GetSelectedDirectory();
        string path = Path.Combine(dir, "NewDialogueData.json");
        path = AssetDatabase.GenerateUniqueAssetPath(path);

        File.WriteAllText(path, TEMPLATE);
        AssetDatabase.Refresh();

        // 选中新文件方便改名
        Object asset = AssetDatabase.LoadAssetAtPath<Object>(path);
        Selection.activeObject = asset;
        EditorGUIUtility.PingObject(asset);

        Debug.Log($"DialogueDataCreator: 已创建 {path}，请重命名并设置 configKey，然后注册到 Addressables");
    }

    private static string GetSelectedDirectory()
    {
        // 优先取 Project 窗口中选中的目录
        foreach (Object obj in Selection.GetFiltered<Object>(SelectionMode.Assets))
        {
            string p = AssetDatabase.GetAssetPath(obj);
            if (AssetDatabase.IsValidFolder(p))
                return p;

            // 如果不是目录，取其父目录
            string parent = Path.GetDirectoryName(p);
            if (!string.IsNullOrEmpty(parent))
                return parent;
        }

        // 兜底：Assets 根目录
        return "Assets";
    }
}
