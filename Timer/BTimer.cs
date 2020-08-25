/*********************************************************
	文件：BTimer
	作者：Administrator
	邮箱：630276388@qq.com
	日期：2020/8/24 11:12:53
	功能：计时器
***********************************************************/
using System;
using System.Collections;
using System.Collections.Generic;
using System.Timers;

namespace BJTimer
{
    public class BTimer
    {
        private Timer serTime;
        private TaskLog taskLog;
        private Action<Action<int>, int> taskHandle;

        private static readonly string obj = "lock";
        private static readonly string lockTime = "lockTime";
        private static readonly string lockFrame = "lockFrame";
        private static readonly string lockDelteId = "lockDelteId";

        private int id;
        private Dictionary<int, TaskFlag> idDic = new Dictionary<int, TaskFlag>();
        private List<int> deleteIds = new List<int>();

        private DateTime startDateTime = new DateTime(1970, 1, 1, 0, 0, 0);
        private double nowTime;

        private List<TimeTask> tempTimeTaskList = new List<TimeTask>();
        private List<TimeTask> timeTaskList = new List<TimeTask>();


        private int sinceframeCount;
        private List<FrameTask> tempFrameTaskList = new List<FrameTask>();
        private List<FrameTask> frameTaskList = new List<FrameTask>();

        public void ResetTimer()
        {
            serTime.Stop();
            idDic.Clear();
            tempTimeTaskList.Clear();
            timeTaskList.Clear();
            tempFrameTaskList.Clear();
            frameTaskList.Clear();
            id = 0;
        }

        public void StartSeverTimer(int interval)
        {
            if (interval != 0)
            {
                if (serTime != null) serTime.Dispose();

                serTime = new Timer(interval);
                serTime.AutoReset = true;
                serTime.Elapsed += (sender, arg) =>
                {
                    Tick();
                };
                serTime.Start();
            }
        }

        public void Tick()
        {
            TimeTaskTick();
            FrameTaskTick();

            if (deleteIds.Count > 0)
            {
                lock (lockDelteId)
                {
                    for (int i = 0; i < deleteIds.Count; i++)
                    {
                        switch (idDic[deleteIds[i]].type)
                        {
                            case TaskType.TimeTask:
                                DealDeleteTimeTask(deleteIds[i]);
                                break;
                            case TaskType.FrameTask:
                                DealDeleteFrameTask(deleteIds[i]);
                                break;
                        }
                        
                    }
                    deleteIds.Clear();
                }
            }
        }

        public void SetLog(TaskLog log)
        {
            taskLog = log;
        }
        private void LogInfo(string info, LogLevel level = LogLevel.Log)
        {
            if (taskLog != null) taskLog.Invoke(info, level);
        }

        public void SetHandle(Action<Action<int>, int> handle)
        {
            taskHandle = handle;
        }

        #region Tool
        public double GetMillisecondsTime()
        {
            return nowTime;
        }

        public DateTime GetLocalDateTime()
        {
            DateTime dt = TimeZone.CurrentTimeZone.ToLocalTime(startDateTime.AddMilliseconds(nowTime));
            return dt;
        }

        public int GetYear()
        {
            return GetLocalDateTime().Year;
        }
        public int GetMonth()
        {
            return GetLocalDateTime().Month;
        }
        public int GetDay()
        {
            return GetLocalDateTime().Day;
        }
        public int GetWeek()
        {
            return (int)GetLocalDateTime().DayOfWeek;
        }

        public string GetLocalTimeStr()
        {
            DateTime dt = GetLocalDateTime();
            string str = GetTimeStr(dt.Hour) + ":" + GetTimeStr(dt.Minute) + ":" + GetTimeStr(dt.Second);
            return str;
        }
        #endregion

        #region TimeTask
        private void TimeTaskTick()
        {
            if(tempTimeTaskList.Count > 0)
            {
                lock (lockTime)
                {
                    for (int i = 0; i < tempTimeTaskList.Count; i++)
                    {
                        AddTimeListItem(tempTimeTaskList[i]);
                    }
                    tempTimeTaskList.Clear();
                }
            }

            nowTime = GetUTCMilliseconds();
            for (int i = 0; i < timeTaskList.Count; i++)
            {
                TimeTask task = timeTaskList[i];
                if (nowTime.CompareTo(task.destTime) < 0) continue;
                else
                {
                    Action<int> cb = task.callBack;
                    try
                    {
                        if(cb != null && taskHandle != null) taskHandle.Invoke(cb, task.id);
                        else if (cb != null) cb.Invoke(task.id);
                    }
                    catch (Exception e)
                    {
                        LogInfo(e.ToString(), LogLevel.Error);
                    }
                }

                if (task.count == 1)
                {
                    //RemoveTimeListItem(task);
                    deleteIds.Add(task.id);
                }
                else
                {
                    if (task.count > 0)
                    {
                        --task.count;
                    }
                    task.destTime += task.delay;
                    timeTaskList[i] = task;
                }
            }
        }

        public IDPack AddTimerTask(Action<int> callBack, double delay, int count = 1, TimeUnit unit = TimeUnit.Millisecound)
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
            nowTime = GetUTCMilliseconds();
            double destTime = nowTime + delay;

            int id = GetId();
            if (id == -1) return new IDPack(id, TaskType.TimeTask);

            idDic[id] = new TaskFlag(idDic[id], TaskType.TimeTask);
            TimeTask task = new TimeTask
            {
                id = id,
                delay = delay,
                destTime = destTime,
                callBack = callBack,
                count = count
            };
            lock (lockTime)
            {
                tempTimeTaskList.Add(task);
            }

            return new IDPack(id, TaskType.TimeTask);
        }

        public void DeleteTimeTask(int id)
        {
            lock (lockDelteId) deleteIds.Add(id);
        }

        private bool DealDeleteTimeTask(int id)
        {
            bool exit = false;
            lock (lockTime)
            {
                if (idDic.ContainsKey(id) && idDic[id].active)
                {
                    exit = true;
                    RemoveTimeListItem(timeTaskList[idDic[id].index]);
                }

                if (!exit)
                {
                    for (int i = 0; i < tempTimeTaskList.Count; i++)
                    {
                        if (tempTimeTaskList[i].id == id)
                        {
                            exit = true;
                            RemoveTimeTaskListItem(tempTimeTaskList, i);
                            /*lock (obj)*/ idDic.Remove(id);
                            break;
                        }
                    }
                }
            }

            return exit;
        }

        public bool ReplaceTimeTask(int id, Action<int> callBack, double delay, int count = 1, TimeUnit unit = TimeUnit.Millisecound)
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
            nowTime = GetUTCMilliseconds();
            double destTime = nowTime + delay;
            TimeTask task = new TimeTask
            {
                id = id,
                delay = delay,
                destTime = destTime,
                callBack = callBack,
                count = count,
            };

            if (idDic.ContainsKey(id) && idDic[id].active)
            {
                timeTaskList[idDic[id].index] = task;
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

        private void RemoveTimeListItem(int index)
        {
            if (timeTaskList.Count == 0 && tempTimeTaskList.Count == 0) return;

            TimeTask task = timeTaskList[index];

            RemoveTimeTaskListItem(timeTaskList, index);

            if (index < timeTaskList.Count)
            {
                TimeTask indexTask = timeTaskList[index];
                TaskFlag flag = new TaskFlag
                {
                    id = indexTask.id,
                    index = index,
                    active = true
                };
                idDic[indexTask.id] = flag;
            }

            /*lock(obj)*/ idDic.Remove(task.id);
        }
        private void RemoveTimeListItem(TimeTask task)
        {
            //Debug.Log(task.index);
            RemoveTimeListItem(idDic[task.id].index);
        }
        private void AddTimeListItem(TimeTask task)
        {
            TaskFlag flag = new TaskFlag
            {
                id = task.id,
                index = timeTaskList.Count,
                active = true
            };
            idDic[task.id] = flag;
            timeTaskList.Add(task);
        }

        private void RemoveTimeTaskListItem(List<TimeTask> list, int index)
        {
            int last = list.Count - 1;
            TimeTask temp = list[index];
            list[index] = list[last];
            list[last] = temp;
            list.RemoveAt(last);
        }
        #endregion

        #region FrameTask
        private void FrameTaskTick()
        {
            if(tempFrameTaskList.Count > 0)
            {
                lock (lockFrame)
                {
                    for (int i = 0; i < tempFrameTaskList.Count; i++)
                    {
                        AddFrameListItem(tempFrameTaskList[i]);
                    }
                    tempFrameTaskList.Clear();
                }
            }


            for (int i = 0; i < frameTaskList.Count; i++)
            {
                FrameTask task = frameTaskList[i];
                if (sinceframeCount < task.destFrame) continue;
                else
                {
                    Action<int> cb = task.callBack;
                    try
                    {
                        if (cb != null && taskHandle != null) taskHandle.Invoke(cb, task.id);
                        else if(cb != null) cb.Invoke(task.id);
                    }
                    catch (Exception e)
                    {
                        LogInfo(e.ToString(), LogLevel.Error);
                    }
                }

                if (task.count == 1)
                {
                    //RemoveFrameListItem(task);
                    deleteIds.Add(task.id);
                }
                else
                {
                    if (task.count > 0)
                    {
                        --task.count;
                    }
                    task.destFrame += task.delay;
                    frameTaskList[i] = task;
                }
            }

            ++sinceframeCount;

            if (deleteIds.Count > 0)
            {
                lock (lockFrame)
                {
                    for (int i = 0; i < deleteIds.Count; i++)
                    {
                        DealDeleteFrameTask(deleteIds[i]);
                    }
                    deleteIds.Clear();
                }
            }
        }

        public IDPack AddFrameTask(Action<int> callBack, int delay, int count = 1)
        {
            int destFrame = sinceframeCount + delay;

            int id = GetId();
            if (id == -1) return new IDPack(id, TaskType.FrameTask);

            idDic[id] = new TaskFlag(idDic[id], TaskType.FrameTask);
            FrameTask task = new FrameTask
            {
                id = id,
                delay = delay,
                destFrame = destFrame,
                callBack = callBack,
                count = count
            };
            lock (lockFrame)
            {
                tempFrameTaskList.Add(task);
            }

            return new IDPack(id, TaskType.FrameTask);
        }

        public void DeleteFrameTask(int id)
        {
            lock (lockDelteId) deleteIds.Add(id);
        }
        
        private bool DealDeleteFrameTask(int id)
        {
            bool exit = false;
            lock (lockFrame)
            {
                if (idDic.ContainsKey(id) && idDic[id].active)
                {
                    exit = true;
                    RemoveFrameListItem(idDic[id].index);
                }

                if (!exit)
                {
                    for (int i = 0; i < tempFrameTaskList.Count; i++)
                    {
                        if (tempFrameTaskList[i].id == id)
                        {
                            exit = true;
                            RemoveFrameTaskListItem(tempFrameTaskList, i);
                            /*lock(obj)*/ idDic.Remove(id);
                            break;
                        }
                    }
                }
            }


            return exit;
        }

        public bool ReplaceFrameTask(int id, Action<int> callBack, int delay, int count = 1)
        {
            int destFrame = sinceframeCount + delay;
            FrameTask task = new FrameTask
            {
                id = id,
                delay = delay,
                destFrame = destFrame,
                callBack = callBack,
                count = count
            };

            if (idDic.ContainsKey(id) && idDic[id].active)
            {
                frameTaskList[idDic[id].index] = task;
                return true;
            }
            else
            {
                for (int i = 0; i < tempFrameTaskList.Count; i++)
                {
                    if (tempFrameTaskList[i].id == id)
                    {
                        tempFrameTaskList[i] = task;
                        return true;
                    }
                }
            }

            return false;
        }

        private void RemoveFrameListItem(int index)
        {
            if (frameTaskList.Count == 0 && tempFrameTaskList.Count == 0) return;

            FrameTask task = frameTaskList[index];

            RemoveFrameTaskListItem(frameTaskList, index);
            if (index < frameTaskList.Count)
            {
                FrameTask indexTask = frameTaskList[index];
                TaskFlag flag = new TaskFlag
                {
                    id = indexTask.id,
                    index = index,
                    active = true
                };
                idDic[indexTask.id] = flag;
            }

            idDic.Remove(task.id);
        }
        private void RemoveFrameListItem(FrameTask task)
        {
            //Debug.Log(task.index);
            RemoveTimeListItem(idDic[task.id].index);
        }
        private void AddFrameListItem(FrameTask task)
        {
            TaskFlag flag = new TaskFlag
            {
                id = task.id,
                index = frameTaskList.Count,
                active = true
            };
            idDic[task.id] = flag;
            frameTaskList.Add(task);
        }

        private void RemoveFrameTaskListItem(List<FrameTask> list, int index)
        {
            int last = list.Count - 1;
            FrameTask temp = list[index];
            list[index] = list[last];
            list[last] = temp;
            list.RemoveAt(last);
        }
        #endregion

        #region Common
        private int GetId()
        {
            lock (obj)
            {
                id += 1;

                int len = 0;
                while (true)
                {
                    if (id == int.MaxValue) id = 0;

                    if (idDic.ContainsKey(id)) id++;
                    else break;

                    len++;
                    if (len == int.MaxValue)
                    {
                        LogInfo("计时任务已满，无法添加任务", LogLevel.Error);
                        return -1;
                    }
                }

                TaskFlag flag = new TaskFlag
                {
                    id = id,
                    active = false
                };
                idDic[id] = flag;
            }

            return id;
        }

        private double GetUTCMilliseconds()
        {
            TimeSpan ts = DateTime.UtcNow - startDateTime;
            return ts.TotalMilliseconds;
        }

        private string GetTimeStr(int time)
        {
            if (time < 10) return "0" + time.ToString();
            else return time.ToString();
        }
        #endregion

        public delegate void TaskLog(string str, LogLevel logLevel = LogLevel.Log);
        public enum LogLevel
        {
            Info,
            Log,
            Warning,
            Error
        }
    }
}

