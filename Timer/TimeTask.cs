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
using UnityEngine;

public struct TimeTask
{
    public int id;
    public int index;
    public float delay;
    public float destTime;
    public Action callBack;
    public int count;
    public bool active;
}

public enum TimeUnit
{
    Millisecound,
    Secound,
    Minute,
    Hour,
    Day
}
