using UnityEngine;
using UnityEngine.EventSystems;

namespace LccHotfix
{
    public class DragEventTrigger : MonoBehaviour, IDragHandler, IEventTrigger<UIEventListener.PointerEventDataDelegate>
    {
        public UIEventListener.PointerEventDataDelegate onDrag;

        public static IEventTrigger<UIEventListener.PointerEventDataDelegate> Get(GameObject go)
        {
            DragEventTrigger listener = go.GetComponent<DragEventTrigger>();
            if (listener == null)
                listener = go.AddComponent<DragEventTrigger>();
            return listener;
        }

        public void OnDrag(PointerEventData eventData)
        {
            if (onDrag != null)
            {
                onDrag(gameObject, eventData);
            }
        }

        public void AddListener(UIEventListener.PointerEventDataDelegate t)
        {
            onDrag += t;
        }

        public void RemoveListener(UIEventListener.PointerEventDataDelegate t)
        {
            onDrag -= t;
        }

        public void RemoveListener()
        {
            onDrag = null;
        }
    }
}