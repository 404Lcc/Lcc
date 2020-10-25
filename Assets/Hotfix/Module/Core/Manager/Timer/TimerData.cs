using System;

namespace LccHotfix
{
    public class TimerData
    {
        public int id;
        public event Action Complete;
        public float start;
        public float end;
        public TimerData(int id, Action complete, float start, float end)
        {
            this.id = id;
            Complete += complete;
            this.start = start;
            this.end = end;
        }
        public void Excute()
        {
            Complete();
        }
        public void Reset()
        {
            id = -1;
            Complete = null;
            start = -1;
            end = -1;
        }
    }
}