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

    private BTimer bTimer;

    public void Init(int interval)
    {
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
        bTimer.StartSeverTimer(interval);
    }

    void TimerTest()
    {
        System.Timers.Timer t = new System.Timers.Timer(50);
    }
}
