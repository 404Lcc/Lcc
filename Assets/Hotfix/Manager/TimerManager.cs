using System.Collections.Generic;
using UnityEngine;

namespace Hotfix
{
    public class TimerManager : MonoBehaviour
    {
        public List<TimerData> timerdatas;
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
            timerdatas = new List<TimerData>();
        }
        public TimerData TimerStart(float delay, TimerDelegate timerdelegate)
        {
            TimerData timer = new TimerData(timerdatas.Count + 1, timerdelegate, Time.time, Time.time + delay);
            timerdatas.Add(timer);
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
            if (index < timerdatas.Count)
            {
                Excute();
            }
        }
        public void Excute()
        {
            TimerData timer = timerdatas[index];
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
            if (index >= timerdatas.Count)
            {
                timerdatas.Clear();
                index = 0;
            }
        }
    }
}