using System;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Model
{
    [Serializable]
    public class LongClickEvent : UnityEvent { }
    public class LongClickButton : Button
    {
        private DateTime _first;
        private DateTime _second;
        [SerializeField]
        public LongClickEvent longClick;
        public override void OnPointerDown(PointerEventData eventData)
        {
            base.OnPointerDown(eventData);
            if (_first.Equals(default))
            {
                _first = DateTime.Now;
            }
        }
        public override void OnPointerUp(PointerEventData eventData)
        {
            base.OnPointerUp(eventData);
            //在鼠标抬起时触发,时差大于200ms
            if (!_first.Equals(default))
            {
                _second = DateTime.Now;
            }
            if (!_first.Equals(default) && !_second.Equals(default))
            {
                TimeSpan time = _second - _first;
                float milliseconds = time.Seconds * 1000 + time.Milliseconds;
                if (milliseconds > 200)
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
            longClick?.Invoke();
            ResetTime();
        }
        private void ResetTime()
        {
            _first = default;
            _second = default;
        }
    }
}