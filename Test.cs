/*********************************************************
	文件：Test
	作者：Administrator
	邮箱：630276388@qq.com
	日期：2020/8/23 13:44:57
	功能：待定
***********************************************************/
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Test : MonoBehaviour
{
    private TimerSystem timeSys;
    private Queue<int> queue = new Queue<int>();
    private float timer;
    // Start is called before the first frame update
    void Start()
    {
        timeSys = TimerSystem.InitSingleton();
        timeSys.InitSys();
    }

    private void Update()
    {
        timeSys.Tick();

        timer += Time.deltaTime;
        int id = timeSys.AddTimerTask(() => { int a = 500 + 500; }, 500, 5);
        queue.Enqueue(id);

        if(timer > 1f)
        {
            timer = 0f;
            int rand = Random.Range(0, queue.Count / 2);
            for(int i = 0; i < rand; i++)
            {
                int tid = queue.Dequeue();
                timeSys.DeleteTimeTask(tid);
            }
        }
    }

}
