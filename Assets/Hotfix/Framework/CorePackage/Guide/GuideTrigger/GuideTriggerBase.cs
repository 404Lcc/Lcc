namespace LccHotfix
{
    public class GuideTriggerBase : GuideCondBase
    {
        public GuideTriggerConfig Config { get; private set; }

        public GuideTriggerBase(GuideTriggerConfig config) : base(config.args)
        {
            Config = config;
        }
    }
}