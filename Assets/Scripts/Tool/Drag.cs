using UnityEngine;
using UnityEngine.EventSystems;

namespace Model
{
    public delegate void VoidDelegate();
    public delegate void VectorDelegate(Vector2 delta);
    public class Drag : EventTrigger
    {
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
                Drag listener = GameUtil.GetComponent<Drag>(go);
                if (listener == null)
                {
                    listener = GameUtil.AddComponent<Drag>(go);
                }
                return listener;
            }
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