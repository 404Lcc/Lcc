namespace Hotfix
{
    public delegate void TimerDelegate();
    public class TimerData
    {
        public int id;
        public TimerDelegate timerDelegate;
        public float start;
        public float end;

        public TimerData(int id, TimerDelegate timerDelegate, float start, float end)
        {
            this.id = id;
            this.timerDelegate = timerDelegate;
            this.start = start;
            this.end = end;
        }
        public void Excute()
        {
            timerDelegate();
        }
        public void Reset()
        {
            id = -1;
            timerDelegate = null;
            start = -1;
            end = -1;
        }
    }
}