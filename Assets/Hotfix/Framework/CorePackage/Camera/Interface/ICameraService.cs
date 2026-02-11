using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace LccHotfix
{
    public interface ICameraService : IService
    {
        Camera MainCamera { get; }
        Camera UICamera { get; }
        Camera AdaptCamera { get; }
        Camera CurrentCamera { get; }
        void AddOverlayCamera(Camera camera);
        void RemoveOverlayCamera(Camera camera);
        void SetCurrentCamera(Camera camera);
    }
}