using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public static class Tools 
{
    //批量执行DontDestroyOnLoad函数
    public static void DontDestoryObjects(Object[] objects)
    {
        foreach (Object obj in objects)
        {
            GameObject.DontDestroyOnLoad(obj);
        }
    }

    #region Debug相关
    //只在编辑器模式打印，减少性能消耗
    public static void Log(string content)
    {
#if UNITY_EDITOR
        Debug.Log(content);
#endif
    }
    #endregion
}
