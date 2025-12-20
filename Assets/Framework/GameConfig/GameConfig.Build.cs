namespace LccModel
{
    public static partial class GameConfig
    {
        /// <summary>
        /// 打包时间戳
        /// </summary>
        public static long BuildTime
        {
            get
            {
                return GetConfig<long>("buildTime");
            }
        }


        public static void ReadBuild(string text)
        {
            long time = long.Parse(text);
            AddConfig("buildTime", time);
        }
    }
}