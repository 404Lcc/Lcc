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
    }
}