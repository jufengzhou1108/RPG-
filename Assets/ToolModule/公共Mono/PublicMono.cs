using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;


//公共mono
//1.用于为非mono提供协程入口
//2.用于批量执行更新函数减少性能消耗
//3.用于为非mono提供周期函数
public class PublicMono : SingletonMono<PublicMono>
{
    private event UnityAction updateActions;

    public void AddUpdateAction<T>(UnityAction action)
    {
        updateActions += action;
    }

    public void RemoveUpdateAction<T>(UnityAction action)
    {
        updateActions -= action;
    }

    private void Update()
    {
        updateActions?.Invoke();
    }
}
