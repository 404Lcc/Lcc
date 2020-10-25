using System;
using UnityEngine;
using UnityEngine.EventSystems;

namespace LccModel
{
    public delegate void PointerHandler(PointerEventData eventData);
    public delegate void DragHandler(Vector2 delta);
    public class DragEventTrigger : EventTrigger
    {
        public event PointerHandler Down;
        public event PointerHandler UP;
        public event Action BeginDrag;
        public event DragHandler Drag;
        public event Action EndDrag;
        public static DragEventTrigger GetDragEventTrigger(GameObject gameObject)
        {
            if (gameObject == null) return null;
            return gameObject.GetComponent<DragEventTrigger>() ?? gameObject.AddComponent<DragEventTrigger>();
        }
        public override void OnPointerDown(PointerEventData eventData)
        {
            Down?.Invoke(eventData);
        }
        public override void OnPointerUp(PointerEventData eventData)
        {
            UP?.Invoke(eventData);
        }
        public override void OnBeginDrag(PointerEventData eventData)
        {
            BeginDrag?.Invoke();
        }
        public override void OnDrag(PointerEventData eventData)
        {
            Drag?.Invoke(eventData.delta);
        }
        public override void OnEndDrag(PointerEventData eventData)
        {
            EndDrag?.Invoke();
        }
    }
}