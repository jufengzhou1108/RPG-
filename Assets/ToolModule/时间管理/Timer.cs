using System;
using System.Collections;
using System.Diagnostics;
using System.Threading.Tasks;
using UnityEngine;

public class UnityTimer 
{
    private long nextTriggerSeconds;
    private Action action;
    private Stopwatch stopwatch = new();
    private bool isRunning = false;
    private int intervalTime;

    //创建循环任务 
    public void StartRepeatTimer(int intervalTime,Action action)
    {
        if (intervalTime == 0)
        {
            UnityEngine.Debug.Log("intervaltime不能为0");
        }

        //先重置计时器
        Reset();

        this.action = action;
        nextTriggerSeconds = 0;
        isRunning = true;

        action?.Invoke();
        nextTriggerSeconds += intervalTime;
        this.intervalTime = intervalTime;
        stopwatch.Start();

        PublicMono.Instance.StartCoroutine( LoopTick());
    }

    //轮询计时
    private IEnumerator LoopTick()
    {
        while (isRunning)
        {
            yield return new WaitForSecondsRealtime(intervalTime/1000f);
            Tick();
        }
    }

    //单次触发事件
    private void Tick()
    {
        while (isRunning&&stopwatch.ElapsedMilliseconds >= nextTriggerSeconds)
        {
            action?.Invoke();
            nextTriggerSeconds += intervalTime;
        }
    }


    public void End()
    {
        isRunning = false;
    }

    private void Reset()
    {
        nextTriggerSeconds = 0;
        action = null;
        stopwatch.Reset();
        isRunning = false;
    }
}
