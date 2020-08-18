using System.Collections.Generic;
using UnityEngine;

namespace Hotfix
{
    public class TimerManager : MonoBehaviour
    {
        public List<TimerData> timerDataList;
        public int index;
        void Awake()
        {
            InitManager();
        }
        void Start()
        {
        }
        void Update()
        {
            AutomaticExcute();
        }
        public void InitManager()
        {
            timerDataList = new List<TimerData>();
        }
        public TimerData TimerStart(float delay, TimerDelegate timerDelegate)
        {
            TimerData timer = new TimerData(timerDataList.Count + 1, timerDelegate, Time.time, Time.time + delay);
            timerDataList.Add(timer);
            return timer;
        }
        public void TimerExcute(TimerData timer)
        {
            timer.Excute();
            timer.Reset();
        }
        public void TimerReset(TimerData timer)
        {
            timer.Reset();
        }
        public void AutomaticExcute()
        {
            if (index < timerDataList.Count)
            {
                Excute();
            }
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