using UnityEngine;
using UnityEngine.EventSystems;

namespace LccHotfix
{
    public class DoubleClickEventTrigger : MonoBehaviour, IPointerClickHandler, IEventTrigger<UIEventListener.IntDelegate>
    {
        private float _checkInterval = 0.5f;
        private float _checkTime;

        public UIEventListener.IntDelegate onDoubleClick;

        public static IEventTrigger<UIEventListener.IntDelegate> Get(GameObject go)
        {
            DoubleClickEventTrigger listener = go.GetComponent<DoubleClickEventTrigger>();
            if (listener == null)
                listener = go.AddComponent<DoubleClickEventTrigger>();
            return listener;
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            if (_checkTime <= 0)
            {
                _checkTime = _checkInterval;
                if (onDoubleClick != null)
                {
                    onDoubleClick(gameObject, 1);
                }
            }
            else
            {
                _checkTime = 0f;
                if (onDoubleClick != null)
                {
                    onDoubleClick(gameObject, 2);
                }
            }
        }

        private void Update()
        {
            if (_checkTime >= 0f)
            {
                _checkTime -= Time.unscaledDeltaTime;
            }
        }

        public void AddListener(UIEventListener.IntDelegate t)
        {
            onDoubleClick += t;
        }

        public void RemoveListener(UIEventListener.IntDelegate t)
        {
            onDoubleClick -= t;
        }

        public void RemoveListener()
        {
            onDoubleClick = null;
        }
    }
}