using System;

namespace LccModel
{
    public static class TimeUtil
    {
        private static readonly DateTime _dt1970 = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        public static long ServerMinusClientTime { private get; set; }
        public static long ClinetTime()
        {
            return (DateTime.UtcNow.Ticks - _dt1970.Ticks) / 10000;
        }
        public static long ServerTime()
        {
            return ClinetTime() + ServerMinusClientTime;
        }
    }
}