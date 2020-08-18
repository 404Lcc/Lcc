using System;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

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
        if (enterClick != null)
        {
            enterClick.Invoke();
        }
    }
    public override void OnPointerExit(PointerEventData eventData)
    {
        base.OnPointerExit(eventData);
        if (exitClick != null)
        {
            exitClick.Invoke();
        }
    }
}