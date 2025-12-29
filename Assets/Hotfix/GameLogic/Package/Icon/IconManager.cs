using System;
using System.Collections.Generic;
using UnityEngine;

namespace LccHotfix
{
    internal class IconManager : Module, IIconService
    {
        internal override void Update(float elapseSeconds, float realElapseSeconds)
        {

        }

        internal override void Shutdown()
        {
        }

        public T GetIcon<T>(Transform parent, float size = 1) where T : IconBase, new()
        {
            if (parent == null)
                return null;

            T icon = ReferencePool.Acquire<T>();
            icon.InitIcon(parent, size);
            return icon;
        }
    }
}