using System;
using UnityEngine;

namespace LccHotfix
{
    public interface ICameraBlender
    {
        public Transform Target { get; set; }
        void PostInitialize();
        void Dispose();
        void Update();
        void LateUpdate();
        void ChangeTarget(Transform target);
    }

    public interface ICameraZoomCtrl
    {
        float CurZoomSize { get; set; }
        float ZoomSizeMin { get; }
        float ZoomSizeMax { get; }
    }
}