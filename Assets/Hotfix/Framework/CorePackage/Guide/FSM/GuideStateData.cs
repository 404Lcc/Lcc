namespace LccHotfix
{
    public class GuideStateData
    {
        public int GuideId { get; private set; }
        public GuideStepConfig Config { get; private set; }

        public bool IsRunning { get; set; } = false;
        public bool IsTimeout { get; set; } = false;
        public bool IsFsmException { get; set; } = false;
        public bool IsFsmFinish { get; set; } = false;
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
            IsFsmException = false;
            IsFsmFinish = false;
            IsPause = false;
        }
    }
}