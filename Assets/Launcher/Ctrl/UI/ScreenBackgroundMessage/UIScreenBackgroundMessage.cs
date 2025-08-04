using UnityEngine;
using System.Collections;
using UnityEngine.EventSystems;
using System;
using System.Collections.Generic;

//所有点击在屏幕非UI处的消息，都会传递到InputManager，进行场景的消息判断。比如是否点击塔。
//也就是说，在屏幕上非UI处点击时，才进行场景消息的处理。
//如果使用Input.GetMouseDown这样的方式处理场景消息，则场景消息会和UI消息重叠。比如在塔上面显示一个菜单，点击菜单会同时触发场景消息和UI消息。

namespace LccModel
{
    public class UIScreenBackgroundMessage : MonoBehaviour
    {
        public static UIScreenBackgroundMessage Instance;

        public GameObject backgroundGo;//UI底层

        //点击场景触发
        public event Action<Vector2, bool> PressEvent;
        public event Action<Vector2> ClickEvent;
        public event Action<Vector2> DragEvent;
        public event Action DragStartEvent;
        public event Action DragEndEvent;
        public event Action<float> ZoomEvent;

        //点击UI和场景都会触发
        public event Action<bool, GameObject> CommonClickEvent;

        private bool isZooming = false;
        private float preABDistance = 0f;


        void Awake()
        {
            Instance = this;

        }

        void Update()
        {
            Zooming();



            if (Input.GetMouseButtonDown(0) || Input.GetMouseButtonDown(1))
            {
                var hitObj = CheckRaycastResult();
                if (hitObj != null)
                {
                    bool hitIsScene = false;
                    if (hitObj.name == backgroundGo.name)
                    {
                        hitIsScene = true;
                    }
                    CommonClickEvent?.Invoke(hitIsScene, hitObj);

                }
            }

        }

        void Zooming()
        {
            if (Input.touchCount == 2)
            {
                Vector2 touchAPos = Input.touches[0].position;
                Vector2 touchBPos = Input.touches[1].position;
                if (isZooming == false)
                {
                    isZooming = true;
                    preABDistance = (touchAPos - touchBPos).sqrMagnitude;
                }
                else
                {
                    float curDist = (touchAPos - touchBPos).sqrMagnitude;
                    OnZooming((curDist - preABDistance) * 0.0000033f);
                    preABDistance = curDist;
                }
            }
            else
                isZooming = false;

            //编辑器下用滚轮缩放
#if UNITY_EDITOR
            if (Input.mouseScrollDelta.y != 0)
                OnZooming(Input.mouseScrollDelta.y * 0.1f);
#endif
        }

        public void OnClick()
        {
            ClickEvent?.Invoke(Input.mousePosition);//如果直接发个OnClick事件过来，就没点在UI上
        }



        public void OnPress(bool isDown)
        {
            PressEvent?.Invoke(Input.mousePosition, isDown);
        }






        public void OnDragStart()
        {
            DragStartEvent?.Invoke();
        }

        public void OnDrag(BaseEventData database)
        {
            var point = (PointerEventData)database;

            var delta = point.delta;

            DragEvent?.Invoke(delta);
        }
        public void OnDragEnd()
        {

            DragEndEvent?.Invoke();
        }


        //delta是两次两手指距离的平方的差
        //delta< 0 手指间距缩小
        //delta > 0 手指间距方大
        void OnZooming(float delta)
        {
            ZoomEvent?.Invoke(delta);
        }







        public GameObject CheckRaycastResult()
        {
            PointerEventData eventData = new PointerEventData(EventSystem.current);
            eventData.pressPosition = Input.mousePosition;
            eventData.position = Input.mousePosition;
            List<RaycastResult> list = new List<RaycastResult>();
            EventSystem.current.RaycastAll(eventData, list);
            if (list.Count > 0)
            {
                return list[0].gameObject;
            }
            else
            {
                return null;
            }
        }
    }
}