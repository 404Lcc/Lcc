using UnityEngine;
using UnityEngine.EventSystems;

namespace Model
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
        public static Drag GetDrag(GameObject go)
        {
            if (go == null)
            {
                return null;
            }
            else
            {
                Drag listener = Util.GetComponent<Drag>(go);
                if (listener == null)
                {
                    listener = Util.AddComponent<Drag>(go);
                }
                return listener;
            }
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