/*********************************************************
	文件：TimeTask
	作者：Administrator
	邮箱：630276388@qq.com
	日期：2020/8/23 14:36:34
	功能：定时任务数据类
***********************************************************/
using System;
using System.Collections;
using System.Collections.Generic;

namespace BJTimer
{
    public struct TimeTask
    {
        public int id;
        public double delay;
        public double destTime;
        public Action<int> callBack;
        public int count;
    }

    public struct FrameTask
    {
        public int id;
        public int curFrame;
        public int delay;
        //public int destFrame;
        public Action<int> callBack;
        public int count;
    }

    public struct TaskFlag
    {
        public int id;
        public int index;
        public TaskType type;
        public bool active;

        public TaskFlag(int id, int index , TaskType type, bool active)
        {
            this.id = id;
            this.index = index;
            this.active = active;
            this.type = type;
        }

        public TaskFlag(TaskFlag flag, bool active)
        {
            id = flag.id;
            index = flag.index;
            type = flag.type;
            this.active = active;
        }
        public TaskFlag(TaskFlag flag, TaskType type)
        {
            id = flag.id;
            index = flag.index;
            this.type = type;
            active = flag.active;
        }
    }

    public struct TaskPack
    {
        public int id;
        public Action<int> callBack;
        public TaskPack(int _id, Action<int> _callBack)
        {
            id = _id;
            callBack = _callBack;
        }
    }

    public struct IDPack
    {
        public int id;
        public TaskType type;

        public IDPack(int id, TaskType type)
        {
            this.id = id;
            this.type = type;
        }
    }

    public enum TimeUnit
    {
        Millisecound,
        Secound,
        Minute,
        Hour,
        Day
    }

    public enum TaskType
    {
        TimeTask,
        FrameTask
    }
}

