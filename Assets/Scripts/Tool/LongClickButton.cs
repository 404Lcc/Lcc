using System;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

[Serializable]
public class LongClickEvent : UnityEvent { }
public class LongClickButton : Button
{
    private DateTime first;
    private DateTime second;
    [SerializeField]
    public LongClickEvent longclick;
    public override void OnPointerDown(PointerEventData eventData)
    {
        base.OnPointerDown(eventData);
        if (first.Equals(default(DateTime)))
        {
            first = DateTime.Now;
        }
    }
    public override void OnPointerUp(PointerEventData eventData)
    {
        base.OnPointerUp(eventData);
        //在鼠标抬起时触发,时差大于200ms
        if (!first.Equals(default(DateTime)))
        {
            second = DateTime.Now;
        }
        if (!first.Equals(default(DateTime)) && !second.Equals(default(DateTime)))
        {
            var time = second - first;
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
        if (longclick != null)
        {
            longclick.Invoke();
        }
        ResetTime();
    }
    private void ResetTime()
    {
        first = default(DateTime);
        second = default(DateTime);
    }
}