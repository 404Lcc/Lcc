﻿using System;
using UnityEngine;
using UnityEngine.EventSystems;

namespace LccModel
{
    public class DragEventTrigger : EventTrigger
    {
        public event Action<PointerEventData> Down;
        public event Action<PointerEventData> UP;
        public event Action<PointerEventData> Drag;
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
        public override void OnDrag(PointerEventData eventData)
        {
            Drag?.Invoke(eventData);
        }
    }
}