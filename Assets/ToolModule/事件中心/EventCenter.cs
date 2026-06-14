using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// 事件中心，负责不同事件的订阅和触发，默认一个事件的参数是固定的
/// </summary>
public class EventCenter : Singleton<EventCenter>
{
    private Dictionary<Type, IEventContainer> eventDic = new();

    /// <summary>
    /// 添加监听
    /// </summary>
    /// <param name="eventName">监听事件名</param>
    /// <param name="action">回调函数</param>
    public void AddListener<T>(UnityAction<T> action) where T : struct
    {
        Type key = typeof(T);
        if (!eventDic.ContainsKey(key))
        {
            EventContainer<T> container = new EventContainer<T>();
            container.action = action;

            eventDic.Add(key, container);
            return;
        }

        (eventDic[key] as EventContainer<T>).action += action;
    }

    /// <summary>
    /// 移除监听
    /// </summary>
    /// <param name="eventName">监听事件名</param>
    /// <param name="action">回调函数</param>
    public void RemoveListener<T>(UnityAction<T> action) where T : struct
    {
        Type key = typeof(T);

        if (!eventDic.ContainsKey(key))
        {
            return;
        }

        (eventDic[key] as EventContainer<T>).action -= action;

        if ((eventDic[key] as EventContainer<T>).action == null)
        {
            eventDic.Remove(key);
        }
    }

    /// <summary>
    /// 触发事件
    /// </summary>
    /// <param name="eventName">事件名</param>
    public void EventTrigger<T>(T args) where T : struct
    {
        Type key = typeof(T);

        if (!eventDic.ContainsKey(key))
        {
            return;
        }

        (eventDic[key] as EventContainer<T>).action?.Invoke(args);
    }

    //事件容器基类
    private interface IEventContainer { }

    //事件容器类(T是参数结构体)
    private class EventContainer<T> : IEventContainer where T : struct
    {
        public UnityAction<T> action;
    }

    /// <summary>
    /// 清空事件中心
    /// </summary>
    public void Clear()
    {
        eventDic.Clear();
    }
}
