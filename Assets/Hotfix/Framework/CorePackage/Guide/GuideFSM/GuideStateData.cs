namespace LccHotfix
{
    public class GuideStateData
    {
        private int _guideId = -1;
        private GuideStateNode _stateConfig;
        public GuideStateNode StateConfig => _stateConfig;
        public int GuideId => _guideId;

        public bool IsFinished = false;
        public bool IsRunning = false;
        public bool IsTimeout = false;
        public bool IsExceptionQuit = false;
        public bool IsFsmOver = false;
        public bool IsForceQuit = false;
        public bool IsPause = false;

        public GuideStateData(int guideId, GuideStateNode stateConfig)
        {
            _guideId = guideId;
            _stateConfig = stateConfig;
            IsTimeout = false;
        }

        public void Reset()
        {
            IsFinished = false;
            IsRunning = false;
            IsTimeout = false;
            IsExceptionQuit = false;
            IsFsmOver = false;
            IsForceQuit = false;
            IsPause = false;
        }
    }
}