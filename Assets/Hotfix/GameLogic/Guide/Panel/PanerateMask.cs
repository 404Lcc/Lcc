using System;
using UnityEngine;
using UnityEngine.EventSystems;
using System.Collections.Generic;

namespace LccHotfix
{
    public class PanerateMask : MonoBehaviour, IPointerClickHandler
    {
        private List<GameObject> _targetList = new List<GameObject>();
        private List<RaycastResult> _rawRaycastList = new List<RaycastResult>();
        private Action _clickMaskCallback;
        private Action _clickTargetCallback;

        public void OnPointerClick(PointerEventData eventData)
        {
            Raycast(eventData);
        }

        public void AddTarget(GameObject item)
        {
            if (item == null)
                return;
            if (_targetList.Contains(item))
                return;

            _targetList.Add(item);
        }

        public void ClearTarget()
        {
            _targetList.Clear();
        }

        public void SetCallback(Action clickMask, Action clickTarget)
        {
            _clickMaskCallback = clickMask;
            _clickTargetCallback = clickTarget;
        }

        private void Raycast(PointerEventData eventData)
        {
            _rawRaycastList.Clear();
            EventSystem.current.RaycastAll(eventData, _rawRaycastList);
            if (_targetList.Count == 0)
                return;

            foreach (var item in _rawRaycastList)
            {
                //遮罩层自身需要添加该脚本，否则会导致ExecuteEvents.Execute再次触发遮罩层自身的IPointerClickHandler导致死循环
                if (item.gameObject.GetComponent<IgnoreEventRaycast>())
                {
                    _clickMaskCallback?.Invoke();
                    continue;
                }

                if (_targetList.Contains(item.gameObject))
                {
                    ExecuteEvents.Execute(item.gameObject, eventData, ExecuteEvents.pointerClickHandler);
                    _clickTargetCallback?.Invoke();
                }
            }
        }
    }
}