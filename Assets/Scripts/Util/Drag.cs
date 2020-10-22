using UnityEngine;
using UnityEngine.EventSystems;

namespace LccModel
{
    public delegate void EventDataDelegate(PointerEventData eventData);
    public delegate void VoidDelegate();
    public delegate void VectorDelegate(Vector2 delta);
    public class Drag : EventTrigger
    {
        public EventDataDelegate down;
        public EventDataDelegate up;
        public VoidDelegate beginDrag;
        public VectorDelegate drag;
        public VoidDelegate endDrag;
        public static Drag GetDrag(GameObject gameObject)
        {
            if (gameObject == null) return null;
            Drag listener = gameObject.GetComponent<Drag>();
            if (listener == null)
            {
                listener = gameObject.AddComponent<Drag>();
            }
            return listener;
        }
        public override void OnPointerDown(PointerEventData eventData)
        {
            down?.Invoke(eventData);
        }
        public override void OnPointerUp(PointerEventData eventData)
        {
            up?.Invoke(eventData);
        }
        public override void OnBeginDrag(PointerEventData eventData)
        {
            beginDrag?.Invoke();
        }
        public override void OnDrag(PointerEventData eventData)
        {
            drag?.Invoke(eventData.delta);
        }
        public override void OnEndDrag(PointerEventData eventData)
        {
            endDrag?.Invoke();
        }
    }
}