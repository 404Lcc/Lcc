using System;
using System.Collections.Generic;

namespace LccHotfix
{
    public class UIViewLoader : ViewLoaderBase
    {
        public string UIName; // UI名称
        public override void Load(LogicEntity entity, Action<LogicEntity, int, IReceiveLoaded> callback)
        {
            if (IsPrepare)
                return;
            IsPrepare = true;
            callback?.Invoke(entity, Category, null);
        }
    }
}