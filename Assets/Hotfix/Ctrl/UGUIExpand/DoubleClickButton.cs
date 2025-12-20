using System;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace LccHotfix
{
    [Serializable]
    public class DoubleClickEvent : UnityEvent { }
    public class DoubleClickButton : Button
    {
        private DateTime _first;
        private DateTime _second;
        [SerializeField]
        public DoubleClickEvent doubleClick;
        public override void OnPointerDown(PointerEventData eventData)
        {
            base.OnPointerDown(eventData);
            if (_first.Equals(default))
            {
                _first = DateTime.Now;
            }
            else
            {
                _second = DateTime.Now;
            }
        }
        public override void OnPointerUp(PointerEventData eventData)
        {
            base.OnPointerUp(eventData);
            //在第二次鼠标抬起时触发,时差小于600ms
            if (!_first.Equals(default) && !_second.Equals(default))
            {
                TimeSpan time = _second - _first;
                float milliseconds = time.Seconds * 1000 + time.Milliseconds;
                if (milliseconds < 600)
                {
                    Press();
                }
                else
                {
                    ResetTime();
                }
            }
        }
        public override void OnPointerExit(PointerEventData eventData)
        {
            base.OnPointerExit(eventData);
            ResetTime();
        }
        private void Press()
        {
            doubleClick?.Invoke();
            ResetTime();
        }
        private void ResetTime()
        {
            _first = default;
            _second = default;
        }
    }
}