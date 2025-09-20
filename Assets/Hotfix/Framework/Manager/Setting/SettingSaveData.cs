namespace LccHotfix
{
    public class SettingSaveData : ISave
    {
        public string TypeName => GetType().FullName;
        public DisplayModeType DisplayModeType { get; set; }
        public ResolutionType ResolutionType { get; set; }

        public void Init()
        {
            DisplayModeType = DisplayModeType.FullScreen;
            ResolutionType = ResolutionType.Resolution1920x1080;
        }
    }
}