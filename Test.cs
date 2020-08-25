/*********************************************************
	文件：Test
	作者：Administrator
	邮箱：630276388@qq.com
	日期：2020/8/23 13:44:57
	功能：待定
***********************************************************/
using BJTimer;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Test : MonoBehaviour
{
    private TimerSystem timeSys;
    private Queue<IDPack> queue = new Queue<IDPack>();
    private float timer;
    // Start is called before the first frame update
    void Start()
    {
        timeSys = TimerSystem.Instance;
        timeSys.Init();
    }

    private void Update()
    {
        timer += Time.deltaTime;
        IDPack id = timeSys.AddTimerTask((tid) => { int a = 500 + 500; }, 500, 5);
        queue.Enqueue(id);

        if(timer > 1f)
        {
            timer = 0f;
            int rand = Random.Range(0, queue.Count / 2);
            for(int i = 0; i < rand; i++)
            {
                IDPack tid = queue.Dequeue();
                if(tid.type == TaskType.TimeTask) timeSys.DeleteTimeTask(tid.id);
            }
        }
    }

}
