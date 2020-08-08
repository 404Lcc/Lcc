using System;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

[Serializable]
public class DoubleClickEvent : UnityEvent { }
public class DoubleClickButton : Button
{
    private DateTime first;
    private DateTime second;
    [SerializeField]
    public DoubleClickEvent doubleclick;
    public override void OnPointerDown(PointerEventData eventData)
    {
        base.OnPointerDown(eventData);
        if (first.Equals(default(DateTime)))
        {
            first = DateTime.Now;
        }
        else
        {
            second = DateTime.Now;
        }
    }
    public override void OnPointerUp(PointerEventData eventData)
    {
        base.OnPointerUp(eventData);
        //在第二次鼠标抬起时触发,时差小于600ms
        if (!first.Equals(default(DateTime)) && !second.Equals(default(DateTime)))
        {
            var time = second - first;
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
        if (doubleclick != null)
        {
            doubleclick.Invoke();
        }
        ResetTime();
    }
    private void ResetTime()
    {
        first = default(DateTime);
        second = default(DateTime);
    }
}