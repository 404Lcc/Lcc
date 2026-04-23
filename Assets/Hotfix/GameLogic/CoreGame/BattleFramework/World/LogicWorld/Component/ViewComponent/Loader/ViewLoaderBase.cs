using System;
using System.Collections.Generic;

namespace LccHotfix
{
    public interface IViewLoader : IReference
    {
        public int Category { get; set; } // View类型，唯一存在。e.g:两把武器的时候，一把叫武器1 一把叫武器2
        public System.Type ViewClassType { get; set; } // View逻辑实现类的ClassType
        public bool IsAsync { get; set; } // 是否是异步加载
        public bool IsForce { get; set; } // 是否强制重新加载
        public List<IViewLoader> SubLoaderList { get; set; } // 子View
        public bool IsPrepare { get; set; }
        public bool IsDeploy { get; set; }

        public void Load(LogicEntity entity, Action<LogicEntity, int, IReceiveLoaded> callback);
    }
    
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
        public Type ViewClassType { get; set; }
        public bool IsAsync { get; set; }
        public bool IsForce { get; set; }
        public List<IViewLoader> SubLoaderList { get; set; }
        public bool IsPrepare { get; set; }
        public bool IsDeploy { get; set; }
        public virtual void Load(LogicEntity entity, Action<LogicEntity, int, IReceiveLoaded> callback)
        {
        }
    }
}