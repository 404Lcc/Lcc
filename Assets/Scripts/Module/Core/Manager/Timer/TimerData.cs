using System;

namespace LccModel
{
    public class TimerData
    {
        public long id;
        public TimerType type;
        public event Action Callback;
        public long time;
        public TimerData()
        {
        }
        public TimerData(long id)
        {
            this.id = id;
        }
        public TimerData(long id, TimerType type, Action callback, long time)
        {
            this.id = id;
            this.type = type;
            Callback = callback;
            this.time = time;
        }
        //public float start;
        //public float end;
        //public TimerData(int id, Action callback, float start, float end)
        //{
        //    this.id = id;
        //    Callback += callback;
        //    this.start = start;
        //    this.end = end;
        //}
        //public void Excute()
        //{
        //    Callback?.Invoke();
        //}
        //public void Reset()
        //{
        //    id = -1;
        //    Callback = null;
        //    start = 0;
        //    end = 0;
        //}
    }
}