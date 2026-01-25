namespace LccHotfix
{
    public interface IGuideService : IService
    {
        void SetGuideCheckFinish(IGuideCheckFinish guideCheckFinish);
        void LoadForceGuideConfigList(GuideConfigList config);

        void LoadForceGuideTriggerConfigList(GuideTriggerConfigList seqConfig);

        void LoadNoForceGuideConfigList(GuideConfigList config);

        void LoadNoForceGuideTriggerConfigList(GuideTriggerConfigList seqConfig);

        void InitGuide();

        void InitGuideTrigger();

        void ResetGuide();
    }
}