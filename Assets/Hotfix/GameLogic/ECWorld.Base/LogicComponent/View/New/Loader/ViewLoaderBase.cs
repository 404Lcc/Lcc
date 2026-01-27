using System;
using System.Collections.Generic;

namespace LccHotfix
{
    public class ViewLoaderBase : IViewLoader
    {
        public void OnRecycle()
        {
            if (SubLoaderList != null)
            {
                for (int i = 0; i < SubLoaderList.Count; i++)
                {
                    ReferencePool.Release(SubLoaderList[i]);
                }
                SubLoaderList.Clear();
                SubLoaderList = null;
            }

            IsPrepare = false;
            IsDeploy = false;
        }

        public int Category { get; set; }
        public Type CategoryType { get; set; }
        public bool IsAsync { get; set; }
        public List<IViewLoader> SubLoaderList { get; set; }
        public bool IsPrepare { get; set; }
        public bool IsDeploy { get; set; }
        public virtual void Load(LogicEntity entity, Action<LogicEntity, int, IReceiveLoaded> callback)
        {
        }
    }
}