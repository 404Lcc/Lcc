using UnityEngine;
using System;
using LccModel;
using UnityEngine.Video;

namespace LccHotfix
{
    internal class InputManager : Module, IInputService
    {
        //点击场景触发
        public event Action<Vector2, bool> PressEvent;
        public event Action<Vector2> ClickEvent;
        public event Action<Vector2> DragEvent;
        public event Action DragStartEvent;
        public event Action DragEndEvent;
        public event Action<float> ZoomEvent;

        //点击UI和场景都会触发
        public event Action<bool, GameObject> CommonClickEvent;

        public InputManager()
        {
            UIScreenBackgroundMessage.Instance.PressEvent += OnPress;
            UIScreenBackgroundMessage.Instance.ClickEvent += OnClick;
            UIScreenBackgroundMessage.Instance.DragEvent += OnDrag;
            UIScreenBackgroundMessage.Instance.DragStartEvent += OnDragStart;
            UIScreenBackgroundMessage.Instance.DragEndEvent += OnDragEnd;
            UIScreenBackgroundMessage.Instance.ZoomEvent += OnZooming;

            UIScreenBackgroundMessage.Instance.CommonClickEvent += OnCommonClick;
        }
        internal override void Shutdown()
        {
            UIScreenBackgroundMessage.Instance.PressEvent -= OnPress;
            UIScreenBackgroundMessage.Instance.ClickEvent -= OnClick;
            UIScreenBackgroundMessage.Instance.DragEvent -= OnDrag;
            UIScreenBackgroundMessage.Instance.DragStartEvent -= OnDragStart;
            UIScreenBackgroundMessage.Instance.DragEndEvent -= OnDragEnd;
            UIScreenBackgroundMessage.Instance.ZoomEvent -= OnZooming;

            UIScreenBackgroundMessage.Instance.CommonClickEvent -= OnCommonClick;
        }

        internal override void Update(float elapseSeconds, float realElapseSeconds)
        {
        }


        //绝大部分都是点击消息
        public void OnClick(Vector2 screenPos)
        {
            // game pause
            if (Time.timeScale == 0)
                return;


            if (ClickEvent != null)
                ClickEvent(screenPos);

        }



        public void OnPress(Vector2 screenPos, bool isDown)
        {
            if (PressEvent != null)
            {
                PressEvent(screenPos, isDown);
            }
        }


        public void OnDragStart()
        {
            if (DragStartEvent != null)
            {
                DragStartEvent();
            }
        }

        public void OnDrag(Vector2 delta)
        {
            if (DragEvent != null)
                DragEvent(delta);
        }

        public void OnDragEnd()
        {
            if (DragEndEvent != null)
            {
                DragEndEvent();
            }

        }

        public void OnZooming(float delta)
        {
            if (ZoomEvent != null)
                ZoomEvent(delta);
        }

        public void OnCommonClick(bool hitIsScene, GameObject hitObj)
        {
            if (CommonClickEvent != null)
                CommonClickEvent(hitIsScene, hitObj);
        }



        // ============================= Raycast ===============================

    }
}