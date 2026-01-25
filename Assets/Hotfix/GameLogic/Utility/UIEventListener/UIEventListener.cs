using UnityEngine;
using UnityEngine.EventSystems;

namespace LccHotfix
{
    public interface IEventTrigger<T>
    {
        void AddListener(T t);
        void RemoveListener();
        void RemoveListener(T t);
    }

    public class UIEventListener
    {

        public delegate void VoidDelegate(GameObject go);

        public delegate void IntDelegate(GameObject go, int value);

        public delegate void BoolDelegate(GameObject go, bool value);

        public delegate void PointerEventDataDelegate(GameObject go, PointerEventData data);

        public static IEventTrigger<VoidDelegate> OnClick(GameObject go)
        {
            return ClickEventTrigger.Get(go);
        }

        public static IEventTrigger<IntDelegate> OnDoubleClick(GameObject go)
        {
            return DoubleClickEventTrigger.Get(go);
        }

        public static IEventTrigger<PointerEventDataDelegate> OnDragBegin(GameObject go)
        {
            return DragBeginEventTrigger.Get(go);
        }

        public static IEventTrigger<PointerEventDataDelegate> OnDrag(GameObject go)
        {
            return DragEventTrigger.Get(go);
        }

        public static IEventTrigger<PointerEventDataDelegate> OnDragEnd(GameObject go)
        {
            return DragEndTrigger.Get(go);
        }

        public static IEventTrigger<PointerEventDataDelegate> OnPointEnter(GameObject go)
        {
            return EnterEventTrigger.Get(go);
        }

        public static IEventTrigger<PointerEventDataDelegate> OnPointUp(GameObject go)
        {
            return LeaveEventTrigger.Get(go);
        }
    }
}