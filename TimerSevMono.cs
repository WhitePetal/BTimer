/*********************************************************
	文件：TimerSevMono
	作者：Administrator
	邮箱：630276388@qq.com
	日期：2020/8/26 12:10:22
	功能：待定
***********************************************************/
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TimerSevMono : MonoBehaviour
{
    private TimerSev timerSev;
    private void Awake()
    {
        DontDestroyOnLoad(gameObject);
    }

    // Start is called before the first frame update
    void Start()
    {
        timerSev = TimerSev.Instance;
    }

    // Update is called once per frame
    void Update()
    {
        timerSev.deltaTime = Time.deltaTime;
        timerSev.Update();
    }
}
