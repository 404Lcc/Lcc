namespace LccModel
{
    public partial class GameConfig
    {
        /// <summary>
        /// 打包时间戳
        /// </summary>
        public long buildTime
        {
            get
            {
                return GetConfig<long>("buildTime");
            }
        }


        public void ReadBuild(string text)
        {
            long time = long.Parse(text);
            AddConfig("buildTime", time);
        }
    }
}