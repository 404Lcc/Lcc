using System;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace LccHotfix
{
    [Serializable]
    public class EnterClickEvent : UnityEvent { }
    [Serializable]
    public class ExitClickEvent : UnityEvent { }
    public class EnterClickButton : Button
    {
        [SerializeField]
        public EnterClickEvent enterClick;
        [SerializeField]
        public ExitClickEvent exitClick;
        public override void OnPointerEnter(PointerEventData eventData)
        {
            base.OnPointerEnter(eventData);
            enterClick?.Invoke();
        }
        public override void OnPointerExit(PointerEventData eventData)
        {
            base.OnPointerExit(eventData);
            exitClick?.Invoke();
        }
    }
}