using System;

namespace LccHotfix
{
    public class TimerData
    {
        public int id;
        public Action action;
        public float start;
        public float end;
        public TimerData(int id, Action action, float start, float end)
        {
            this.id = id;
            this.action = action;
            this.start = start;
            this.end = end;
        }
        public void Excute()
        {
            action();
        }
        public void Reset()
        {
            id = -1;
            action = null;
            start = -1;
            end = -1;
        }
    }
}