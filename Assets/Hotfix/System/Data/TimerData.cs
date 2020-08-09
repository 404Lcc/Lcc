namespace Hotfix
{
    public delegate void TimerDelegate();
    public class TimerData
    {
        public int id;
        public TimerDelegate timerdelegate;
        public float start;
        public float end;

        public TimerData(int id, TimerDelegate timerdelegate, float start, float end)
        {
            this.id = id;
            this.timerdelegate = timerdelegate;
            this.start = start;
            this.end = end;
        }
        public void Excute()
        {
            timerdelegate();
        }
        public void Reset()
        {
            id = -1;
            timerdelegate = () => { };
            start = -1;
            end = -1;
        }
    }
}