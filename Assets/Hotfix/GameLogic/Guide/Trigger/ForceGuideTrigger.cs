namespace LccHotfix
{
    public class TestTrigger : GuideTriggerCondBase
    {
        public TestTrigger(GuideTriggerConfig cfg) : base(cfg)
        {
        }

        public override bool CheckTrigger()
        {
            return false;
        }
    }
}