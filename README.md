# BTimer
高效低GC的Unity/C#计时系统
* 双端通用: 基于C#实现，可运行在服务器和多线程环境，也可以运行在Unity客户端环境中。
* 高效低GC: 算法优化较为高效，且之后在计时任务完成销毁时产生一个委托的GC。下图给出了一段示例代码：我们每帧创建一个计时任务，并在一秒后销毁掉一半计时任务，Unity Profiler显示GC为0：
// 待添加
* 使用方便: 直接通过提供的 System 单例类，即可使用计时、计时回调、计算日期时间等功能。并且计时功能支持真实时间计时和帧计时。

# 使用示例：
## Unity客户端：
首先通过 TimerSystem.Instance 获取单例引用，之后通过 Init 对计时系统进行初始化，然后 StartTimer 即可启动计时系统：
<details>
<summary>代码示例</summary>
```java
    void Start()
    {
        timeSys = TimerSystem.Instance;
        timeSys.Init();

        timeSys.StartTimer();
    }
```
</details>
  之后可以通过 TimerSystem 的 Add、Delete、Replace 来 添加、删除、替换 时间计时或帧计时任务：
<details>
<summary>代码示例</summary>
```java
    private void Update()
    {
        timer += Time.deltaTime;
        IDPack id = timeSys.AddTimerTask((tid) => {int a = tid + 100; }, 500, 5);
        queue.Enqueue(id);

        if(timer > 1f)
        {
            timer = 0f;
            for(int i = 0; i < queue.Count / 2; i++)
            {
                IDPack tid = queue.Dequeue();
                if(tid.type == TaskType.TimeTask) timeSys.DeleteTimeTask(tid.id);
            }
        }

        IDPack pack = queue.Dequeue();
        timeSys.ReplaceTimeTask(pack.id, (tid) => { }, 2, 1, TimeUnit.Secound);
    }
```
</details>
  TimerSystem 还提供了一些工具类方法，具体可在代码中查看。
  ## 服务端：
  服务端计时系统的启动与客户端大体相同，区别在于 服务端计时系统的 Init 函数有两个参数，第一个参数表示计时系统的帧率，即多少毫秒为一帧； 另一个是默认值为 true 的布尔参数——dealInMain，该参数默认或者传入true表示计时任务的回调函数即具体任务是在主线程中进行的，多线程中只负责进行计时部分。而传入false则表示计时部分和任务执行部分都将在多线程中进行：
<details>
<summary>代码示例</summary>
```java
        static void Main(string[] args)
        {
            timerSystem = ServerTimerSystem.Instance;
            timerSystem.Init(20, true);
            timerSystem.StartTimer();
        }
```
</details>
  需要注意的是，如果我们 Init 中传入的 dealInMain 为 true，则我们需要自行在主线程中的合适位置对计时器进行驱动(调用 DealTask 方法)：
<details>
<summary>代码示例</summary>
```java
        static void Main(string[] args)
        {
            timerSystem = ServerTimerSystem.Instance;
            timerSystem.Init(20, true);
            timerSystem.StartTimer();
            while (true)
            {
                timerSystem.DealTask();
            }
```
</details>
  之后其它的操作与客户端完全相同：
<details>
<summary>代码示例</summary>
```java
        static void Main(string[] args)
        {
            timerSystem = ServerTimerSystem.Instance;
            timerSystem.Init(20, true);
            timerSystem.StartTimer();

            while (true)
            {
                if (!delete)
                {
                    IDPack id = timerSystem.AddbTimerTask((tid) => { Console.WriteLine(tid); }, 1, 0);
                    IDPack fid = timerSystem.AddFrameTask((tid) => { Console.WriteLine(tid); }, 1, 0);
                    queue.Enqueue(id);
                    queue.Enqueue(fid);
                    Console.WriteLine("ADD");
                }


                timer += 1;
                timerSystem.DealTask();
                if(timer > 100 && !delete)
                {
                    while(queue.Count > 0)
                    {
                        IDPack pack = queue.Dequeue();
                        switch (pack.type)
                        {
                            case TaskType.TimeTask:
                                timerSystem.DeleteTimeTask(pack.id);
                                break;
                            case TaskType.FrameTask:
                                timerSystem.DeleteFrameTask(pack.id);
                                break;
                        }
                    }
                    Console.WriteLine("_________________________________________");
                    timer = 0;
                    delete = true;
                }
            }
        }
```
</details>
  ## 关于日志(Log):
  计时系统内部可能会输出一些错误日志：
  在Unity 客户端系统中，日志输出默认使用 UnityEngine.Debug
  在服务端，日志输出默认使用 Console.WriteLine
  可以使用对应环境的 System 的 SetLog 方法来自定义日志输出方式(例如，对于服务端就调用 ServerTimeSystem.Instance.SetLog((str, level) => {...}))
  SetLog 方法需要传入一个自定义的委托，该委托接收两个参数：输出日志的字符串信息 和 输出日志的安全等级，示例：
<details>
<summary>代码示例</summary>
```java
    void Start()
    {
        timeSys = TimerSystem.Instance;
        timeSys.Init();

        timeSys.SetLog((str, level) =>
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

        timeSys.StartTimer();
    }
```
</details>
  注意，应当在计时器启动之前进行日志设置
