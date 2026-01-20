namespace LccHotfix
{
    public class GuideStateData
    {
        public int GuideId { get; private set; }
        public GuideStepConfig Config { get; private set; }

        public bool IsRunning { get; set; } = false;
        public bool IsTimeout { get; set; } = false;
        public bool IsExceptionQuit { get; set; } = false;
        public bool IsFsmFinish { get; set; } = false;
        public bool IsForceQuit { get; set; } = false;
        public bool IsPause { get; set; } = false;

        public GuideStateData(int guideId, GuideStepConfig config)
        {
            GuideId = guideId;
            Config = config;
        }

        public void Reset()
        {
            IsRunning = false;
            IsTimeout = false;
            IsExceptionQuit = false;
            IsFsmFinish = false;
            IsForceQuit = false;
            IsPause = false;
        }
    }
}