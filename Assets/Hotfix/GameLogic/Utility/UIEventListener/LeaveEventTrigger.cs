using UnityEngine;
using UnityEngine.EventSystems;

namespace LccHotfix
{
    public class LeaveEventTrigger : MonoBehaviour, IPointerUpHandler, IEventTrigger<UIEventListener.PointerEventDataDelegate>
    {
        public UIEventListener.PointerEventDataDelegate onClick;

        public static IEventTrigger<UIEventListener.PointerEventDataDelegate> Get(GameObject go)
        {
            LeaveEventTrigger listener = go.GetComponent<LeaveEventTrigger>();
            if (listener == null)
                listener = go.AddComponent<LeaveEventTrigger>();
            return listener;
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            if (onClick != null)
            {
                onClick(gameObject, eventData);
            }
        }

        public void AddListener(UIEventListener.PointerEventDataDelegate t)
        {
            onClick += t;
        }

        public void RemoveListener(UIEventListener.PointerEventDataDelegate t)
        {
            onClick -= t;
        }

        public void RemoveListener()
        {
            onClick = null;
        }
    }
}