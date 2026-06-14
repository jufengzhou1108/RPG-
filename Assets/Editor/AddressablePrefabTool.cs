using UnityEngine;
using UnityEditor;
using UnityEditor.AddressableAssets;
using UnityEditor.AddressableAssets.Settings;

/// <summary>
/// 编辑器工具：将选中的预制体添加到 Addressables，键名为预制体名称
/// 在 Project 面板中选中预制体 → 右键 → Addressables → 添加到 Addressables
/// </summary>
public static class AddressablePrefabTool
{
    private const string MenuPath = "Assets/Addressables/添加到 Addressables";

    [MenuItem(MenuPath, false, 20)]
    private static void AddSelectedPrefabToAddressables()
    {
        // 获取 Project 面板中选中的对象
        Object[] selectedObjects = Selection.objects;

        if (selectedObjects == null || selectedObjects.Length == 0)
        {
            EditorUtility.DisplayDialog("提示", "请先在 Project 面板中选中预制体", "确定");
            return;
        }

        AddressableAssetSettings settings = AddressableAssetSettingsDefaultObject.Settings;

        if (settings == null)
        {
            EditorUtility.DisplayDialog("错误", "未找到 Addressables 配置文件，请先初始化 Addressables", "确定");
            return;
        }

        // 获取默认分组（没有则创建一个）
        AddressableAssetGroup group = settings.DefaultGroup;
        if (group == null)
        {
            group = settings.CreateGroup("Default Local Group", false, false, false,
                settings.DefaultGroup.Schemas, typeof(AddressableAssetGroupSchema));
        }

        int successCount = 0;
        int skipCount = 0;

        foreach (Object obj in selectedObjects)
        {
            string assetPath = AssetDatabase.GetAssetPath(obj);
            string guid = AssetDatabase.AssetPathToGUID(assetPath);

            if (string.IsNullOrEmpty(guid))
            {
                continue;
            }

            // 查找是否已有该资源的 Addressable 条目
            AddressableAssetEntry existingEntry = settings.FindAssetEntry(guid);
            if (existingEntry != null)
            {
                // 已存在，跳过
                skipCount++;
                continue;
            }

            // 创建 Addressable 条目，地址使用预制体名称
            AddressableAssetEntry entry = settings.CreateOrMoveEntry(guid, group);
            entry.address = obj.name;

            successCount++;
        }

        // 标记配置为脏，保存
        if (successCount > 0)
        {
            settings.SetDirty(AddressableAssetSettings.ModificationEvent.EntryMoved, null, true);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }

        string message = $"成功添加 {successCount} 个资源到 Addressables";
        if (skipCount > 0)
        {
            message += $"，跳过 {skipCount} 个已存在的资源";
        }
        Debug.Log(message);
    }

    /// <summary>
    /// 验证菜单项是否可用（仅当有选中对象时启用）
    /// </summary>
    [MenuItem(MenuPath, true)]
    private static bool ValidateAddSelectedPrefabToAddressables()
    {
        return Selection.objects != null && Selection.objects.Length > 0;
    }
}
