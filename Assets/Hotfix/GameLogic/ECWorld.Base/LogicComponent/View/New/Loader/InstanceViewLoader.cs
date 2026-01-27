using System;
using System.Collections.Generic;

namespace LccHotfix
{
    public class InstanceViewLoader : ViewLoaderBase
    {
        public int Index;

        public override void Load(LogicEntity entity, Action<LogicEntity, int, IReceiveLoaded> callback)
        {
            if (IsPrepare)
            {
                return;
            }

            IsPrepare = true;
        }
    }
}