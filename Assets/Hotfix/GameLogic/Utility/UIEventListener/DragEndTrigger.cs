using UnityEngine;
using UnityEngine.EventSystems;

namespace LccHotfix
{
    public class DragEndTrigger : MonoBehaviour, IEndDragHandler, IEventTrigger<UIEventListener.PointerEventDataDelegate>
    {
        public UIEventListener.PointerEventDataDelegate onDragEnd;

        public static IEventTrigger<UIEventListener.PointerEventDataDelegate> Get(GameObject go)
        {
            DragEndTrigger listener = go.GetComponent<DragEndTrigger>();
            if (listener == null)
                listener = go.AddComponent<DragEndTrigger>();
            return listener;
        }

        public void OnEndDrag(PointerEventData eventData)
        {
            if (onDragEnd != null)
            {
                onDragEnd(gameObject, eventData);
            }
        }

        public void AddListener(UIEventListener.PointerEventDataDelegate t)
        {
            onDragEnd += t;
        }

        public void RemoveListener(UIEventListener.PointerEventDataDelegate t)
        {
            onDragEnd -= t;
        }

        public void RemoveListener()
        {
            onDragEnd = null;
        }
    }
}