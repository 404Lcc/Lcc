namespace LccHotfix
{
    public interface IGuideService : IService
    {
        void SetGuideCheckFinish(IGuideCheckFinish guideCheckFinish);
        void LoadForceGuideConfig(GuideConfigList config);

        void LoadForceGuideSeqTriggerConfig(GuideSeqConfig seqConfig);

        void LoadNoForceGuideConfig(GuideConfigList config);

        void LoadNoForceGuideSeqTriggerConfig(GuideSeqConfig seqConfig);

        void InitGuide();

        void InitGuideTrigger();
    }
}