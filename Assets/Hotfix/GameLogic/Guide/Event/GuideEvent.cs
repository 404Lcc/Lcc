using LccModel;

namespace LccHotfix
{
    /// <summary>
    /// 点击强制引导完成
    /// </summary>
    public class EvtClickForceGuideFinish : IEventMessage
    {
        public static void Broadcast()
        {
            var evt = new EvtClickForceGuideFinish();
            GameUtility.Dispatch(evt);
        }
    }
}