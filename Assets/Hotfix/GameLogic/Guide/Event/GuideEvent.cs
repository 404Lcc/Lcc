using LccModel;
using UnityEngine;

namespace LccHotfix
{
    /// <summary>
    /// 强制引导
    /// </summary>
    public class EvtShowForceGuide : IEventMessage
    {
        public string guidePath;
        public GuideMaskType type;
        public Vector2 handOffsetPos;

        public static void Broadcast(string guidePath, GuideMaskType type = GuideMaskType.Rectangle, Vector2 handOffsetPos = new Vector2())
        {
            var evt = new EvtShowForceGuide();
            evt.guidePath = guidePath;
            evt.type = type;
            evt.handOffsetPos = handOffsetPos;
            GameUtility.Dispatch(evt);
        }
    }


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