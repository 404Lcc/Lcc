namespace LccModel
{
    public class TimerData : AObjectBase
    {
        public TimerType timerType;
        public long time;
        public object obj;
        public override void InitData(object[] datas)
        {
            base.InitData(datas);

            timerType = (TimerType)datas[0];
            time = (long)datas[1];
            obj = datas[2];
        }
    }
}