namespace LccModel
{
    public class TimerData
    {
        public long id;
        public TimerType timerType;
        public long startTime;
        public long time;
        public int type;
        public object obj;
        public static TimerData Create(long id, TimerType timerType, long startTime, long time, int type, object obj)
        {
            TimerData data = new TimerData();
            data.id = id;
            data.timerType = timerType;
            data.startTime = startTime;
            data.obj = obj;
            data.time = time;
            data.type = type;
            return data;
        }
    }
}