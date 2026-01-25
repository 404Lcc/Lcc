using UnityEngine;
using UnityEngine.EventSystems;

namespace LccHotfix
{
    public class DragBeginEventTrigger : MonoBehaviour, IBeginDragHandler, IEventTrigger<UIEventListener.PointerEventDataDelegate>
    {
        public UIEventListener.PointerEventDataDelegate onDragBegin;

        public static IEventTrigger<UIEventListener.PointerEventDataDelegate> Get(GameObject go)
        {
            DragBeginEventTrigger listener = go.GetComponent<DragBeginEventTrigger>();
            if (listener == null)
                listener = go.AddComponent<DragBeginEventTrigger>();
            return listener;
        }

        public void OnBeginDrag(PointerEventData eventData)
        {
            if (onDragBegin != null)
            {
                onDragBegin(gameObject, eventData);
            }
        }

        public void AddListener(UIEventListener.PointerEventDataDelegate t)
        {
            onDragBegin += t;
        }

        public void RemoveListener(UIEventListener.PointerEventDataDelegate t)
        {
            onDragBegin -= t;
        }

        public void RemoveListener()
        {
            onDragBegin = null;
        }
    }
}