using LccModel;
using UnityEngine;

namespace LccHotfix
{
    /// <summary>
    /// 引导开始
    /// </summary>
    public class EvtGuideStart : IEventMessage
    {
        public int id;

        public static void Broadcast(int id)
        {
            var evt = new EvtGuideStart();
            evt.id = id;
            GameUtility.Dispatch(evt);
        }
    }

    /// <summary>
    /// 引导结束
    /// </summary>
    public class EvtGuideEnd : IEventMessage
    {
        public int id;
        public bool isException;

        public static void Broadcast(int id, bool isException)
        {
            var evt = new EvtGuideEnd();
            evt.id = id;
            evt.isException = isException;
            GameUtility.Dispatch(evt);
        }
    }

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