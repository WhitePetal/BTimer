/*********************************************************
	文件：ServerTimerSystem
	作者：Administrator
	邮箱：630276388@qq.com
	日期：2020/8/24 15:50:44
	功能：待定
***********************************************************/
using System;
using System.Collections;
using System.Collections.Generic;
using BJTimer;

public class ServerTimerSystem
{
    private static ServerTimerSystem instance;
    public static ServerTimerSystem Instance
    {
        get
        {
            if (instance == null) instance = new ServerTimerSystem();
            return instance;
        }
    }

    private static readonly string obj = "lockTaskQueue";

    private BTimer bTimer;
    private Queue<TaskPack> taskPackQue = new Queue<TaskPack>();

    private int interval;

    public void Init(int interval, bool dealInMain = true)
    {
        this.interval = interval;
        bTimer = new BTimer();
        bTimer.SetLog((str, level) =>
        {
            switch (level)
            {
                case BTimer.LogLevel.Info:
                    Console.WriteLine("Info: " + str);
                    break;
                case BTimer.LogLevel.Log:
                    Console.WriteLine("Log: " + str);
                    break;
                case BTimer.LogLevel.Warning:
                    Console.WriteLine("Warning: " + str);
                    break;
                case BTimer.LogLevel.Error:
                    Console.WriteLine("Error: " + str);
                    break;
            }
        });

        // 设置主线程处理句柄，如果调用了该方法
        // 则必须在主线程循环调用 DealTimeTask() 来在主线程处理计时任务回调
        // 如果不调用该方法，则计时任务回调是在多线程中执行
        if (dealInMain)
        {
            bTimer.SetHandle((cb, id) =>
            {
                if (cb != null)
                {
                    lock (obj)
                    {
                        taskPackQue.Enqueue(new TaskPack(id, cb));
                    }
                }
            });
        } 
    }

    public void SetLog(BTimer.TaskLog log)
    {
        bTimer.SetLog(log);
    }

    public void StartTimer()
    {
        bTimer.StartSeverTimer(interval);
    }

    public void StopTimer()
    {
        bTimer.StopServerTimer();
    }

    public void Tick()
    {
        bTimer.Tick();
    }

    public void DealTask()
    {
        while (taskPackQue.Count > 0)
        {
            TaskPack tp;
            lock (obj)
            {
                tp = taskPackQue.Dequeue();
            }
            tp.callBack.Invoke(tp.id);
        }
    }

    public void ResetTimer()
    {
        bTimer.ResetTimer();
    }

    #region TimeTask
    public IDPack AddbTimerTask(Action<int> callBack, double delay, int count = 1, TimeUnit unit = TimeUnit.Millisecound)
    {
        return bTimer.AddTimerTask(callBack, delay, count, unit);
    }

    public void DeleteTimeTask(int id)
    {
        bTimer.DeleteTimeTask(id);
    }

    public bool ReplaceTimeTask(int id, Action<int> callBack, double delay, int count = 1, TimeUnit unit = TimeUnit.Millisecound)
    {
        return bTimer.ReplaceTimeTask(id, callBack, delay, count, unit);
    }

    #endregion

    #region FrameTask
    public IDPack AddFrameTask(Action<int> callBack, int delay, int count = 1)
    {
        return bTimer.AddFrameTask(callBack, delay, count);
    }

    public void DeleteFrameTask(int id)
    {
        bTimer.DeleteFrameTask(id);
    }

    public bool ReplaceFrameTask(int id, Action<int> callBack, int delay, int count = 1)
    {
        return bTimer.ReplaceFrameTask(id, callBack, delay, count);
    }
    #endregion

    #region Tools
    public double GetMillisecondsTime()
    {
        return bTimer.GetMillisecondsTime();
    }

    public DateTime GetLocalDateTime()
    {
        return bTimer.GetLocalDateTime();
    }

    public int GetYear()
    {
        return bTimer.GetYear();
    }
    public int GetMonth()
    {
        return bTimer.GetMonth();
    }
    public int GetDay()
    {
        return bTimer.GetDay();
    }
    public int GetWeek()
    {
        return bTimer.GetWeek();
    }

    public string GetLocalTimeStr()
    {
        return bTimer.GetLocalTimeStr();
    }
    #endregion
}
