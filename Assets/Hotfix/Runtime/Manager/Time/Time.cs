using System;

namespace LccHotfix
{
    internal class Time : Module
    {
        public static Time Instance { get; } = Entry.GetModule<Time>();

        private int timeZone;



        private DateTime dt1970;
        private DateTime dt;

        public long ServerMinusClientTime { private get; set; }

        public long FrameTime;

        public int TimeZone
        {
            get
            {
                return this.timeZone;
            }
            set
            {
                this.timeZone = value;
                dt = dt1970.AddHours(TimeZone);
            }
        }

        public Time()
        {
            this.dt1970 = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
            this.dt = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
            this.FrameTime = this.ClientNow();
        }

        internal override void Update(float elapseSeconds, float realElapseSeconds)
        {
        }

        internal override void Shutdown()
        {
            timeZone = 0;
            dt1970 = default;
            dt = default;
            ServerMinusClientTime = 0;
            FrameTime = 0;
        }


        public void Update()
        {
            this.FrameTime = this.ClientNow();
        }

        /// <summary>
        /// 根据时间戳获取时间
        /// </summary>
        public DateTime ToDateTime(long timeStamp)
        {
            return dt.AddTicks(timeStamp * 10000);
        }

        // 线程安全
        public long ClientNow()
        {
            return (DateTime.UtcNow.Ticks - this.dt1970.Ticks) / 10000;
        }

        public long ServerNow()
        {
            return ClientNow() + ServerMinusClientTime;
        }

        public long ClientFrameTime()
        {
            return this.FrameTime;
        }

        public long ServerFrameTime()
        {
            return this.FrameTime + ServerMinusClientTime;
        }

        public long Transition(DateTime d)
        {
            return (d.Ticks - dt.Ticks) / 10000;
        }
    }
}