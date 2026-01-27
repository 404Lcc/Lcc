using System;
using System.Collections.Generic;

namespace LccHotfix
{
    public interface IViewLoader : IReference
    {
        public int Category { get; set; } // View类型，唯一存在。e.g:两把武器的时候，一把叫武器1 一把叫武器2
        public System.Type CategoryType { get; set; } // View需要加载的type
        public bool IsAsync { get; set; } // 是否是异步加载
        public List<IViewLoader> SubLoaderList { get; set; } // 子View
        public bool IsPrepare { get; set; }
        public bool IsDeploy { get; set; }

        public void Load(LogicEntity entity, Action<LogicEntity, int, IReceiveLoaded> callback);
    }
}