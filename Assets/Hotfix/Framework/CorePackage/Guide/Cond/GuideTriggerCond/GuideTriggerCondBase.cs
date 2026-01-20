namespace LccHotfix
{
    public class GuideTriggerCondBase
    {
        public GuideTriggerConfig Config { get; private set; }

        public GuideTriggerCondBase(GuideTriggerConfig config)
        {
            Config = config;
        }

        public virtual bool CheckTrigger()
        {
            return false;
        }

        public virtual void Release()
        {
        }
    }
}