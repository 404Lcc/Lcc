using System;

namespace LccModel
{
    public class TimerData
    {
        public int id = -1;
        public event Action Callback;
        public float start;
        public float end;
        public TimerData(int id, Action callback, float start, float end)
        {
            this.id = id;
            Callback += callback;
            this.start = start;
            this.end = end;
        }
        public void Excute()
        {
            Callback?.Invoke();
        }
        public void Reset()
        {
            id = -1;
            Callback = null;
            start = 0;
            end = 0;
        }
    }
}