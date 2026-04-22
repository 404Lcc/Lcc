namespace LccHotfix
{
    public interface IGuideService : IService
    {
        void SetGuideCheckFinish(IGuideCheckFinish guideCheckFinish);
        void SetGuidePersistence(IGuidePersistence guidePersistence);
        void SetGuideMessage(IGuideMessage guideMessage);
        void LoadForceGuideConfigList(GuideConfigList config);

        void LoadForceGuideTriggerConfigList(GuideTriggerConfigList seqConfig);

        void LoadNoForceGuideConfigList(GuideConfigList config);

        void LoadNoForceGuideTriggerConfigList(GuideTriggerConfigList seqConfig);

        void InitGuide();

        void InitGuideTrigger();

        bool CheckGuideFinish(int guideId);

        void ReAddGuideTrigger(int guideId);

        void ResetGuide();
    }
}
