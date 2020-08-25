/*********************************************************
	文件：TimerSystem
	作者：Administrator
	邮箱：630276388@qq.com
	日期：2020/8/23 14:23:32
	功能：待定
***********************************************************/
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using BJTimer;

public class TimerSystem : MonoBehaviour
{
    private static TimerSystem instance;
    public static TimerSystem Instance
    {
        get
        {
            if(instance == null)
            {
                instance = new GameObject("TimerSystem").AddComponent<TimerSystem>();
            }

            return instance;
        }
    }

    private BTimer timer;

    public void Init()
    {
        timer = new BTimer();
        timer.SetLog((str, level) =>
        {
            switch (level)
            {
                case BTimer.LogLevel.Info:
                    Debug.Log(str);
                    break;
                case BTimer.LogLevel.Log:
                    Debug.Log(str);
                    break;
                case BTimer.LogLevel.Warning:
                    Debug.LogWarning(str);
                    break;
                case BTimer.LogLevel.Error:
                    Debug.LogError(str);
                    break;
            }
        });
    }

    private void Update()
    {
        timer.Tick();
    }

    public void ResetTimer()
    {
        timer.ResetTimer();
    }

    #region TimeTask
    public IDPack AddTimerTask(Action<int> callBack, double delay, int count = 1, TimeUnit unit = TimeUnit.Millisecound)
    {
        return timer.AddTimerTask(callBack, delay, count, unit);
    }

    public void DeleteTimeTask(int id)
    {
        timer.DeleteTimeTask(id);
    }

    public bool ReplaceTimeTask(int id, Action<int> callBack, double delay, int count = 1, TimeUnit unit = TimeUnit.Millisecound)
    {
        return timer.ReplaceTimeTask(id, callBack, delay, count, unit);
    }

    #endregion

    #region FrameTask
    public IDPack AddFrameTask(Action<int> callBack, int delay, int count = 1)
    {
        return timer.AddFrameTask(callBack, delay, count);
    }

    public void DeleteFrameTask(int id)
    {
        timer.DeleteFrameTask(id);
    }

    public bool ReplaceFrameTask(int id, Action<int> callBack, int delay, int count = 1)
    {
        return timer.ReplaceFrameTask(id, callBack, delay, count);
    }
    #endregion
}
