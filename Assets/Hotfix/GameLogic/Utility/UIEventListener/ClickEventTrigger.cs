using UnityEngine;
using UnityEngine.EventSystems;

namespace LccHotfix
{
    public class ClickEventTrigger : MonoBehaviour, IPointerClickHandler, IEventTrigger<UIEventListener.VoidDelegate>
    {
        private float _clickInterval = 0.1f;
        private float _lastClickTime;

        public UIEventListener.VoidDelegate onClick;

        public static IEventTrigger<UIEventListener.VoidDelegate> Get(GameObject go)
        {
            ClickEventTrigger listener = go.GetComponent<ClickEventTrigger>();
            if (listener == null)
                listener = go.AddComponent<ClickEventTrigger>();
            return listener;
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            if (Time.realtimeSinceStartup - _lastClickTime < _clickInterval)
            {
                return;
            }

            _lastClickTime = Time.realtimeSinceStartup;

            if (onClick != null)
            {
                onClick(gameObject);
            }
        }

        public void AddListener(UIEventListener.VoidDelegate t)
        {
            onClick += t;
        }

        public void RemoveListener(UIEventListener.VoidDelegate t)
        {
            onClick -= t;
        }

        public void RemoveListener()
        {
            onClick = null;
        }
    }
}