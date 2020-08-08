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
    public EnterClickEvent enterclick;
    [SerializeField]
    public ExitClickEvent exitclick;
    public override void OnPointerEnter(PointerEventData eventData)
    {
        base.OnPointerEnter(eventData);
        if (enterclick != null)
        {
            enterclick.Invoke();
        }
    }
    public override void OnPointerExit(PointerEventData eventData)
    {
        base.OnPointerExit(eventData);
        if (exitclick != null)
        {
            exitclick.Invoke();
        }
    }
}