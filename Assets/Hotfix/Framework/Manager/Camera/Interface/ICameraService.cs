using System;
using LccModel;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace LccHotfix
{
    public interface ICameraService : IService
    {
        Camera MainCamera { get; }
        Camera UICamera { get; }
        Camera CurrentCamera { get; set; }
        void AddOverlayCamera(Camera camera);
        void RemoveOverlayCamera(Camera camera);
    }
}