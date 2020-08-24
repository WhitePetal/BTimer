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

        private static readonly string obj = "lock";
        private int id;
        private Dictionary<int, TaskFlag> idDic = new Dictionary<int, TaskFlag>();

        private DateTime startDateTime = new DateTime(1970, 1, 1, 0, 0, 0);
        private double nowTime;

        private List<TimeTask> tempTimeTaskList = new List<TimeTask>();
        private List<TimeTask> timeTaskList = new List<TimeTask>();


        private int sinceframeCount;
        private List<FrameTask> tempFrameTaskList = new List<FrameTask>();
        private List<FrameTask> frameTaskList = new List<FrameTask>();

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
        }

        public void SetLog(TaskLog log)
        {
            taskLog = log;
        }
        private void LogInfo(string info, LogLevel level)
        {
            if (taskLog != null) taskLog.Invoke(info, level);
        }

        #region TimeTask
        private void TimeTaskTick()
        {
            nowTime = GetUTCMilliseconds();
            for (int i = 0; i < tempTimeTaskList.Count; i++)
            {
                AddTimeListItem(tempTimeTaskList[i]);
            }
            tempTimeTaskList.Clear();

            for (int i = 0; i < timeTaskList.Count; i++)
            {
                TimeTask task = timeTaskList[i];
                if (nowTime.CompareTo(task.destTime) < 0) continue;
                else
                {
                    Action cb = task.callBack;
                    try
                    {
                        if (cb != null) cb.Invoke();
                    }
                    catch (Exception e)
                    {
                        LogInfo(e.ToString(), LogLevel.Error);
                    }
                }

                if (task.count == 1)
                {
                    RemoveTimeListItem(task);
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

        public int AddTimerTask(Action callBack, double delay, int count = 1, TimeUnit unit = TimeUnit.Millisecound)
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
            if (id == -1) return id;

            TimeTask task = new TimeTask
            {
                id = id,
                delay = delay,
                destTime = destTime,
                callBack = callBack,
                count = count
            };
            TaskFlag flag = new TaskFlag
            {
                id = id,
                active = false
            };
            tempTimeTaskList.Add(task);
            idDic[id] = flag;

            return id;
        }

        public bool DeleteTimeTask(int id)
        {
            bool exit = false;
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
                        idDic.Remove(id);
                        break;
                    }
                }
            }

            return exit;
        }

        public bool ReplaceTimeTask(int id, Action callBack, double delay, int count = 1, TimeUnit unit = TimeUnit.Millisecound)
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

            idDic.Remove(task.id);
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
            for (int i = 0; i < tempFrameTaskList.Count; i++)
            {
                AddFrameListItem(tempFrameTaskList[i]);
            }
            tempFrameTaskList.Clear();

            for (int i = 0; i < frameTaskList.Count; i++)
            {
                FrameTask task = frameTaskList[i];
                if (sinceframeCount < task.destFrame) continue;
                else
                {
                    Action cb = task.callBack;
                    try
                    {
                        if (cb != null) cb.Invoke();
                    }
                    catch (Exception e)
                    {
                        LogInfo(e.ToString(), LogLevel.Error);
                    }
                }

                if (task.count == 1)
                {
                    RemoveFrameListItem(task);
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
        }

        public int AddFrameTask(Action callBack, int delay, int count = 1)
        {
            int destFrame = sinceframeCount + delay;

            int id = GetId();
            if (id == -1) return id;

            FrameTask task = new FrameTask
            {
                id = id,
                delay = delay,
                destFrame = destFrame,
                callBack = callBack,
                count = count
            };
            TaskFlag flag = new TaskFlag
            {
                id = id,
                active = false
            };
            tempFrameTaskList.Add(task);
            idDic[id] = flag;

            return id;
        }

        public bool DeleteFrameTask(int id)
        {
            bool exit = false;
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
                        idDic.Remove(id);
                        break;
                    }
                }
            }

            return exit;
        }

        public bool ReplaceFrameTask(int id, Action callBack, int delay, int count = 1)
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
            }

            return id;
        }

        private double GetUTCMilliseconds()
        {
            TimeSpan ts = DateTime.UtcNow - startDateTime;
            return ts.TotalMilliseconds;
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

