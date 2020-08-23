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

public class TimerSystem : SystemBase<TimerSystem>
{
    private List<TimeTask> tempTimeTaskList = new List<TimeTask>();
    private List<TimeTask> timeTaskList = new List<TimeTask>();
    private Dictionary<int, TimeTask> idDic = new Dictionary<int, TimeTask>();
    private List<int> usedIds = new List<int>();
    private static readonly string obj = "lock";
    private int id;

    public override void InitSys()
    {
        base.InitSys();
    }

    public void Tick()
    {
        for(int i = 0; i < tempTimeTaskList.Count; i++)
        {
            AddTimeListItem(tempTimeTaskList[i]);
        }
        tempTimeTaskList.Clear();

        for(int i = 0; i < timeTaskList.Count; i++)
        {
            TimeTask task = timeTaskList[i];
            if (Time.realtimeSinceStartup * 1000 < task.destTime) continue;
            else
            {
                Action cb = task.callBack;
                if (cb != null) cb.Invoke();
            }

            if(task.count == 1)
            {
                RemoveTimeListItem(task);
            }
            else
            {
                if(task.count > 0)
                {
                    --task.count;
                }
                task.destTime += task.delay;
                timeTaskList[i] = task;
            }
        }

        for(int i = 0; i < usedIds.Count; i++)
        {
            int id = usedIds[i];
            if (!idDic[id].active)
            {
                idDic.Remove(id);
                RemoveListItem(usedIds, i);
            }
        }
    }

    public int AddTimerTask(Action callBack, float delay, int count = 1, TimeUnit unit = TimeUnit.Millisecound)
    {
        switch (unit)
        {
            case TimeUnit.Millisecound:
                break;
            case TimeUnit.Secound:
                delay *= 1000;
                break;
            case TimeUnit.Minute:
                delay *= 1000 * 60;
                break;
            case TimeUnit.Hour:
                delay *= 1000 * 60 * 60;
                break;
            case TimeUnit.Day:
                delay *= 1000 * 60 * 60 * 24; // 最大支持 24天
                break;
        }
        float destTime = Time.realtimeSinceStartup + delay;

        int id = GetId();
        if (id == -1) return id;

        TimeTask task = new TimeTask
        {
            id = id,
            delay = delay,
            destTime = destTime,
            callBack = callBack,
            count = count,
            active = true
        };
        tempTimeTaskList.Add(task);
        idDic[id] = task;
        usedIds.Add(id);

        return id;
    }

    public bool DeleteTimeTask(int id)
    {
        bool exit = false;
        if (idDic.ContainsKey(id) && idDic[id].active)
        {
            exit = true;
            RemoveTimeListItem(idDic[id]);
        }

        if (!exit)
        {
            for(int i = 0; i < tempTimeTaskList.Count; i++)
            {
                if(tempTimeTaskList[i].id == id)
                {
                    exit = true;
                    int last = tempTimeTaskList.Count - 1;
                    TimeTask taks = tempTimeTaskList[i];
                    tempTimeTaskList[i] = tempTimeTaskList[last];
                    tempTimeTaskList[last] = taks;
                    tempTimeTaskList.RemoveAt(last);
                    break;
                }
            }
        }

        return exit;
    }

    public bool ReplaceTimeTask(int id, Action callBack, float delay, int count = 1, TimeUnit unit = TimeUnit.Millisecound)
    {
        switch (unit)
        {
            case TimeUnit.Millisecound:
                break;
            case TimeUnit.Secound:
                delay *= 1000;
                break;
            case TimeUnit.Minute:
                delay *= 1000 * 60;
                break;
            case TimeUnit.Hour:
                delay *= 1000 * 60 * 60;
                break;
            case TimeUnit.Day:
                delay *= 1000 * 60 * 60 * 24; // 最大支持 24天
                break;
        }
        float destTime = Time.realtimeSinceStartup + delay;
        TimeTask task = new TimeTask
        {
            id = id,
            index = idDic[id].index,
            delay = delay,
            destTime = destTime,
            callBack = callBack,
            count = count,
            active = true
        };

        if (idDic.ContainsKey(id) && idDic[id].active)
        {
            tempTimeTaskList[task.index] = task;
            idDic[id] = task;
            return true;
        }
        else
        {
            for (int i = 0; i < tempTimeTaskList.Count; i++)
            {
                if (tempTimeTaskList[i].id == id)
                {
                    tempTimeTaskList[i] = task;
                    return true;
                }
            }
        }

        return false;
    }

    private int GetId()
    {
        lock (obj)
        {
            id += 1;

            int len = 0;
            while (true)
            {
                if (id == int.MaxValue) id = 0;

                if (idDic.ContainsKey(id) && idDic[id].active) id++;
                else break;

                len++;
                if(len == int.MaxValue)
                {
                    NETCommon.Log("计时任务已满，无法添加任务", NETLogLevel.Error);
                    return -1;
                }
            }
        }

        return id;
    }
    private void RemoveTimeListItem(int index)
    {
        if (timeTaskList.Count == 0) return;

        TimeTask task = timeTaskList[index];
        task.active = false;

        RemoveListItem(timeTaskList, index);
        if(index < timeTaskList.Count)
        {
            TimeTask indexTask = timeTaskList[index];
            indexTask.index = index;
            timeTaskList[index] = indexTask;
        }

        idDic[task.id] = task;
    }
    private void RemoveTimeListItem(TimeTask task)
    {
        //Debug.Log(task.index);
        RemoveTimeListItem(task.index);
    }
    private void AddTimeListItem(TimeTask task)
    {
        task.index = timeTaskList.Count;
        timeTaskList.Add(task);
    }

    private void RemoveListItem<T>(List<T> list, int index)
    {
        int last = list.Count - 1;
        T temp = list[index];
        list[index] = list[last];
        list[last] = temp;
        list.RemoveAt(last);
    }
}
