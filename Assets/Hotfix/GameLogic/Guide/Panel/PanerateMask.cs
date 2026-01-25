using UnityEngine;
using UnityEngine.EventSystems;
using System.Collections.Generic;

namespace LccHotfix
{
    public class PanerateMask : MonoBehaviour, IPointerClickHandler, IPointerDownHandler, IDragHandler
    {
        private List<GameObject> _targetList = new List<GameObject>();
        private List<RaycastResult> _rawRaycastList = new List<RaycastResult>();

        public void OnPointerClick(PointerEventData eventData)
        {
            Raycast(eventData);
        }

        public void AddTarget(GameObject item, UIEventListener.VoidDelegate callback)
        {
            if (item == null)
                return;
            if (_targetList.Contains(item))
                return;

            _targetList.Add(item);
            UIEventListener.OnClick(item).AddListener(callback);
        }

        public void ClearTarget(UIEventListener.VoidDelegate callback)
        {
            foreach (var item in _targetList)
            {
                UIEventListener.OnClick(item).RemoveListener(callback);
            }

            _targetList.Clear();
        }

        private void Raycast(PointerEventData eventData)
        {
            _rawRaycastList.Clear();
            EventSystem.current.RaycastAll(eventData, _rawRaycastList);
            if (_targetList.Count == 0)
                return;

            foreach (var item in _rawRaycastList)
            {
                //Debug.Log(item.gameObject);
                //遮罩层自身需要添加该脚本，否则会导致ExecuteEvents.Execute再次触发遮罩层自身的IPointerClickHandler导致死循环
                if (item.gameObject.GetComponent<IgnoreEventRaycast>())
                {
                    continue;
                }

                if (_targetList.Contains(item.gameObject))
                {
                    ExecuteEvents.ExecuteHierarchy(item.gameObject, eventData, ExecuteEvents.pointerClickHandler);
                }
            }
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            _rawRaycastList.Clear();
            EventSystem.current.RaycastAll(eventData, _rawRaycastList);
            if (_targetList.Count == 0)
                return;
            foreach (var item in _rawRaycastList)
            {
                //Debug.Log(item.gameObject);
                //遮罩层自身需要添加该脚本，否则会导致ExecuteEvents.Execute再次触发遮罩层自身的IPointerClickHandler导致死循环
                if (item.gameObject.GetComponent<IgnoreEventRaycast>())
                {
                    continue;
                }

                if (_targetList.Contains(item.gameObject))
                {
                    ExecuteEvents.ExecuteHierarchy(item.gameObject, eventData, ExecuteEvents.pointerDownHandler);
                    ExecuteEvents.ExecuteHierarchy(item.gameObject, eventData, ExecuteEvents.pointerUpHandler);
                }
            }
        }

        public void OnDrag(PointerEventData eventData)
        {
            _rawRaycastList.Clear();
            EventSystem.current.RaycastAll(eventData, _rawRaycastList);
            if (_targetList.Count == 0)
                return;
            foreach (var item in _rawRaycastList)
            {
                //Debug.Log(item.gameObject);
                //遮罩层自身需要添加该脚本，否则会导致ExecuteEvents.Execute再次触发遮罩层自身的IPointerClickHandler导致死循环
                if (item.gameObject.GetComponent<IgnoreEventRaycast>())
                {
                    continue;
                }

                if (_targetList.Contains(item.gameObject))
                {
                    ExecuteEvents.ExecuteHierarchy(item.gameObject, eventData, ExecuteEvents.dragHandler);
                }
            }
        }
    }
}