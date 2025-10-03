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
        void ShakeCamera(float intensity = 0.5f, float duration = 0.5f);
    }

    public interface ICameraZoomCtrl
    {
        float CurZoomSize { get; set; }
        float ZoomSizeMin { get; }
        float ZoomSizeMax { get; }
    }
}