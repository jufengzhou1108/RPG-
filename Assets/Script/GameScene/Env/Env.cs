using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 游戏场景环境管理器
/// 通过 AddressableManager 加载/移除场景预制体，管理场景资源的生命周期
/// </summary>
public class EnvManager : MonoBehaviour
{
    // 已加载的环境预制体实例（键名 → GameObject 实例）
    private Dictionary<string, GameObject> envInstanceDic = new Dictionary<string, GameObject>();

    /// <summary>
    /// 异步加载环境预制体到场景中
    /// </summary>
    /// <param name="prefabName">Addressable 键名（与预制体名称一致）</param>
    /// <param name="position">放置位置，默认原点</param>
    /// <param name="rotation">放置旋转，默认无旋转</param>
    /// <param name="parent">父节点，默认挂载到当前 Transform</param>
    public void LoadEnv(string prefabName, Vector3? position = null, Quaternion? rotation = null, Transform parent = null)
    {
        if (string.IsNullOrEmpty(prefabName))
        {
            Debug.LogWarning("EnvManager.LoadEnv: prefabName 为空");
            return;
        }

        // 已加载则跳过，避免重复
        if (envInstanceDic.ContainsKey(prefabName))
        {
            Debug.LogWarning($"EnvManager.LoadEnv: \"{prefabName}\" 已存在，跳过加载");
            return;
        }

        Vector3 pos = position ?? Vector3.zero;
        Quaternion rot = rotation ?? Quaternion.identity;
        Transform p = parent ?? transform;

        AddressableManager.Instance.LoadResAsync<GameObject>(prefabName, (prefab) =>
        {
            if (prefab == null)
            {
                Debug.LogError($"EnvManager.LoadEnv: 加载 \"{prefabName}\" 失败");
                return;
            }

            GameObject instance = Instantiate(prefab, pos, rot, p);
            instance.name = prefabName;
            envInstanceDic[prefabName] = instance;
        });
    }

    /// <summary>
    /// 同步加载环境预制体（会阻塞直到加载完成）
    /// </summary>
    public void LoadEnvSync(string prefabName, Vector3? position = null, Quaternion? rotation = null, Transform parent = null)
    {
        if (string.IsNullOrEmpty(prefabName))
        {
            Debug.LogWarning("EnvManager.LoadEnvSync: prefabName 为空");
            return;
        }

        if (envInstanceDic.ContainsKey(prefabName))
        {
            Debug.LogWarning($"EnvManager.LoadEnvSync: \"{prefabName}\" 已存在，跳过加载");
            return;
        }

        Vector3 pos = position ?? Vector3.zero;
        Quaternion rot = rotation ?? Quaternion.identity;
        Transform p = parent ?? transform;

        GameObject prefab = AddressableManager.Instance.LoadRes<GameObject>(prefabName);
        if (prefab == null)
        {
            Debug.LogError($"EnvManager.LoadEnvSync: 加载 \"{prefabName}\" 失败");
            return;
        }

        GameObject instance = Instantiate(prefab, pos, rot, p);
        instance.name = prefabName;
        envInstanceDic[prefabName] = instance;
    }

    /// <summary>
    /// 移除指定环境预制体实例
    /// </summary>
    /// <param name="prefabName">预制体名称</param>
    public void RemoveEnv(string prefabName)
    {
        if (!envInstanceDic.ContainsKey(prefabName))
        {
            Debug.LogWarning($"EnvManager.RemoveEnv: \"{prefabName}\" 未加载，无需移除");
            return;
        }

        // 销毁 GameObject 实例
        GameObject instance = envInstanceDic[prefabName];
        if (instance != null)
        {
            Destroy(instance);
        }

        envInstanceDic.Remove(prefabName);

        // 释放 Addressable 资源句柄
        AddressableManager.Instance.Release<GameObject>(prefabName);
    }

    /// <summary>
    /// 清除所有已加载的环境资源
    /// </summary>
    public void ClearAllEnv()
    {
        foreach (var kvp in envInstanceDic)
        {
            if (kvp.Value != null)
            {
                Destroy(kvp.Value);
            }

            // 释放每个资源的句柄
            AddressableManager.Instance.Release<GameObject>(kvp.Key);
        }

        envInstanceDic.Clear();
        Debug.Log("EnvManager: 已清除所有环境资源");
    }

    void OnDestroy()
    {
        // 场景销毁时清理所有实例（不释放 Addressable 句柄，避免其他地方引用失效）
        foreach (var kvp in envInstanceDic)
        {
            if (kvp.Value != null)
            {
                Destroy(kvp.Value);
            }
        }
        envInstanceDic.Clear();
    }
}
