namespace LccModel
{
    public class TimerData
    {
        public long id;
        public TimerType timerType;
        public float duration;
        public float time;
        public int type;
        public object obj;
        public static TimerData Create(long id, TimerType timerType, float duration, float time, int type, object obj)
        {
            TimerData data = new TimerData();
            data.id = id;
            data.timerType = timerType;
            data.duration = duration;
            data.time = time;
            data.type = type;
            data.obj = obj;
            return data;
        }
    }
}