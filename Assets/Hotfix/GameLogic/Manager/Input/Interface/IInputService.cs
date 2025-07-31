using System;
using System.Collections;
using System.Collections.Generic;
using System.Net.Security;
using System.Reflection;
using System.Security.Cryptography.X509Certificates;
using cfg;
using LccModel;
using Luban;
using SimpleJSON;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Video;

namespace LccHotfix
{
    public interface IInputService : IService
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
        
        //绝大部分都是点击消息
        public void OnClick(Vector2 screenPos);
        public void OnPress(Vector2 screenPos, bool isDown);
        public void OnDragStart();
        public void OnDrag(Vector2 delta);
        public void OnDragEnd();
        public void OnZooming(float delta);
        public void OnCommonClick(bool hitIsScene, GameObject hitObj);
    }
}