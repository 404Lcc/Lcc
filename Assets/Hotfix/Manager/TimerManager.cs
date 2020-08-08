using System.Collections.Generic;
using UnityEngine;

namespace Hotfix
{
    public class TimerManager : MonoBehaviour
    {
        public List<TimerEvent> timerlist;
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
            timerlist = new List<TimerEvent>();
        }
        public TimerEvent TimerStart(float delay, TimerDelegate timerdelegate)
        {
            TimerEvent timer = new TimerEvent(timerlist.Count + 1, timerdelegate, Time.time, Time.time + delay);
            timerlist.Add(timer);
            return timer;
        }
        public void TimerExcute(TimerEvent timer)
        {
            timer.Excute();
            timer.Reset();
        }
        public void TimerReset(TimerEvent timer)
        {
            timer.Reset();
        }
        public void AutomaticExcute()
        {
            if (index < timerlist.Count)
            {
                Excute();
            }
        }
        public void Excute()
        {
            TimerEvent timer = timerlist[index];
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
            if (index >= timerlist.Count)
            {
                timerlist.Clear();
                index = 0;
            }
        }
    }
}