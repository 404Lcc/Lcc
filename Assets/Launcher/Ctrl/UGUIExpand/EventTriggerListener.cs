using System;
using UnityEngine;
using UnityEngine.EventSystems;

namespace LccModel
{
    public class EventTriggerListener : EventTrigger
    {
        public Action<GameObject> onClick;
        public Action<GameObject> onDown;
        public Action<GameObject> onEnter;
        public Action<GameObject> onExit;
        public Action<GameObject> onUP;
        public Action<GameObject> onSelect;
        public Action<GameObject> onUpdateSelect;
        public Action<GameObject> onDrag;

        public static EventTriggerListener Get(GameObject gameObject)
        {
            if (gameObject == null)
                return null;
            return gameObject.GetComponent<EventTriggerListener>() ?? gameObject.AddComponent<EventTriggerListener>();
        }

        public override void OnPointerClick(PointerEventData eventData)
        {
            if (onClick != null)
            {
                onClick(gameObject);
            }
        }

        public override void OnPointerDown(PointerEventData eventData)
        {
            if (onDown != null)
            {
                onDown(gameObject);
            }
        }

        public override void OnPointerEnter(PointerEventData eventData)
        {
            if (onEnter != null)
            {
                onEnter(gameObject);
            }
        }

        public override void OnPointerExit(PointerEventData eventData)
        {
            if (onExit != null)
            {
                onExit(gameObject);
            }
        }

        public override void OnPointerUp(PointerEventData eventData)
        {
            if (onUP != null)
            {
                onUP(gameObject);
            }
        }

        public override void OnSelect(BaseEventData eventBaseData)
        {
            if (onSelect != null)
            {
                onSelect(gameObject);
            }
        }

        public override void OnUpdateSelected(BaseEventData eventBaseData)
        {
            if (onUpdateSelect != null)
            {
                onUpdateSelect(gameObject);
            }
        }
        public override void OnDrag(PointerEventData eventData)
        {
            if (onDrag != null)
            {
                onDrag(gameObject);
            }
        }
    }
}