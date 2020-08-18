using System;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

[Serializable]
public class RightClickEvent : UnityEvent { }
public class RightClickButton : Button
{
    [SerializeField]
    public RightClickEvent rightClick;
    public override void OnPointerDown(PointerEventData eventData)
    {
        base.OnPointerDown(eventData);
        if (Input.GetMouseButtonDown(1))
        {
            Press();
        }
    }
    private void Press()
    {
        if (rightClick != null)
        {
            rightClick.Invoke();
        }
    }
}