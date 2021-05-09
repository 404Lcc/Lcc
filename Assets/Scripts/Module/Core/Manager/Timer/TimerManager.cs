using System;
using System.Collections.Generic;
using UnityEngine;

namespace LccModel
{
    public class TimerManager : Singleton<TimerManager>
    {
        //public List<TimerData> timerList = new List<TimerData>();
        //public int index;
        //public override void Update()
        //{
        //    AutomaticExcute();
        //}
        //public TimerData AddTimer(float delay, Action callback)
        //{
        //    TimerData timer = new TimerData(timerList.Count + 1, callback, Time.time, Time.time + delay);
        //    timerList.Add(timer);
        //    return timer;
        //}
        //public void Excute(TimerData timer)
        //{
        //    timer.Excute();
        //    timer.Reset();
        //}
        //public void AutomaticExcute()
        //{
        //    if (index < timerList.Count)
        //    {
        //        Excute();
        //    }
        //}
        //public void Reset(TimerData timer)
        //{
        //    timer.Reset();
        //}
        //public void Excute()
        //{
        //    TimerData timer = timerList[index];
        //    if (timer.id == -1)
        //    {
        //        Next();
        //        return;
        //    }
        //    if (Time.time >= timer.end)
        //    {
        //        timer.Excute();
        //        Next();
        //    }
        //}
        //public void Next()
        //{
        //    index += 1;
        //    if (index >= timerList.Count)
        //    {
        //        timerList.Clear();
        //        index = 0;
        //    }
        //}
    }
}