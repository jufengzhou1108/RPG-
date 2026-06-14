using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.Events;
using UnityEngine.ResourceManagement.AsyncOperations;
using static UnityEngine.Rendering.VirtualTexturing.Debugging;


/// <summary>
/// Addressable相关的管理类
/// </summary>
public class AddressableManager :Singleton<AddressableManager>
{ 
    //资源权柄字典
    private Dictionary<string,AsyncOperationHandle> handleDic=new Dictionary<string, AsyncOperationHandle>();
    //资源计数字典
    private Dictionary<string, int> numDic = new Dictionary<string, int>();

    /// <summary>
    /// 异步加载可寻址资源
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="name">资源名</param>
    /// <param name="action">资源加载的回调函数</param>
    public void LoadResAsync<T>(string name, UnityAction<T> action) where T : UnityEngine.Object
    { 
        string key=name+"_"+typeof(T).Name;

        AsyncOperationHandle handle;
        if (!handleDic.ContainsKey(key))
        {
            handle= Addressables.LoadAssetAsync<T>(name);
            handleDic.Add(key, handle);
            numDic.Add(key, 0);
        }
        handle=handleDic[key];
        numDic[key]++;

        //如果未加载完只添加回调
        if (!handle.IsDone)
        {
            handle.Completed += (temHandle) =>
            {
                if (temHandle.Status == AsyncOperationStatus.Failed)
                {
                    Debug.Log("资源加载失败"+key);
                    return;
                }
                action?.Invoke(temHandle.Result as T);
            };
            return;
        }

        if (handle.Status == AsyncOperationStatus.Failed)
        {
            Debug.Log("资源加载失败" + key);
            return;
        }
        action?.Invoke(handle.Result as T);
    }

    /// <summary>
    /// 同步加载可寻址资源
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="name">资源名</param>
    /// <returns></returns>
    public T LoadRes<T>(string name) where T : UnityEngine.Object
    {
        string key = name + "_" + typeof(T).Name;

        AsyncOperationHandle handle;
        if (!handleDic.ContainsKey(key))
        {
            handle = Addressables.LoadAssetAsync<T>(name);
            handleDic.Add(key, handle);
            numDic.Add(key, 0);
        }
        handle = handleDic[key];
        numDic[key]++;

        //如果未加载完则等待加载完成
        if (!handle.IsDone)
        {
            handle.WaitForCompletion();
        }

        if (handle.Status == AsyncOperationStatus.Failed)
        {
            Debug.Log("资源加载失败" + key);
            return null;
        }
        return handleDic[key].Result as T;
    }

    //释放资源
    public void Release<T>(string name) where T : UnityEngine.Object
    {
        string key= name + "_" + typeof(T).Name;

        if (!numDic.ContainsKey(key))
        {
            return;
        }

        numDic[key]--;
        if (numDic[key] <= 0)
        {
            handleDic[key].Release();
            handleDic.Remove(key);
            numDic.Remove(key);
        }
    }

    //清空资源
    public void Clear()
    {
        foreach(AsyncOperationHandle handle in handleDic.Values)
        {
            handle.Release();
        }

        handleDic.Clear();
        numDic.Clear();
    }
}
