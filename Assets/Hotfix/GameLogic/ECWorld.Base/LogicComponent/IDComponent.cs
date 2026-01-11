using Entitas;
using UnityEngine;

namespace LccHotfix
{
    public class IDComponent : LogicComponent
    {
        public long id { get; private set; }

        public void Init(long id)
        {
            this.id = id;
        }
    }

    public partial class LogicEntity
    {
        public IDComponent comID { get { return (IDComponent)GetComponent(LogicComponentsLookup.ComID); } }
        public bool hasComID { get { return HasComponent(LogicComponentsLookup.ComID); } }

        public void AddComID(long newId)
        {
            var index = LogicComponentsLookup.ComID;
            if (index < 0)
            {
                Debug.LogError("AddComID 未初始化的组件索引 LogicComponentsLookup.ComID");
                return;
            }
            var component = (IDComponent)CreateComponent(index, typeof(IDComponent));
            component.Init(newId);
            AddComponent(index, component);
        }

        public long ID
        {
            get
            {
                if (hasComID)
                {
                    return comID.id;
                }
                return creationIndex;
            }
        } 
    }


    public static partial class LogicComponentsLookup
    {
        private static ComponentTypeIndex _ComIDIndex = new (typeof(IDComponent));
        public static int ComID => _ComIDIndex.Index;
    }

}