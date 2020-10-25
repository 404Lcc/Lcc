using System;
using System.Collections.Generic;
using UnityEngine;

namespace LccHotfix
{
    public class TimerManager : Singleton<TimerManager>
    {
        public List<TimerData> timerDataList = new List<TimerData>();
        public int index;
        public override void Update()
        {
            AutomaticExcute();
        }
        public TimerData TimerStart(float delay, Action complete)
        {
            TimerData timer = new TimerData(timerDataList.Count + 1, complete, Time.time, Time.time + delay);
            timerDataList.Add(timer);
            return timer;
        }
        public void Excute(TimerData timer)
        {
            timer.Excute();
            timer.Reset();
        }
        public void AutomaticExcute()
        {
            if (index < timerDataList.Count)
            {
                Excute();
            }
        }
        public void Reset(TimerData timer)
        {
            timer.Reset();
        }
        public void Excute()
        {
            TimerData timer = timerDataList[index];
            if (timer.id == -1)
            {
                Next();
                return;
            }
            if (Time.time >= timer.end)
            {
                timer.Excute();
                Next();
            }
        }
        public void Next()
        {
            index += 1;
            if (index >= timerDataList.Count)
            {
                timerDataList.Clear();
                index = 0;
            }
        }
    }
}