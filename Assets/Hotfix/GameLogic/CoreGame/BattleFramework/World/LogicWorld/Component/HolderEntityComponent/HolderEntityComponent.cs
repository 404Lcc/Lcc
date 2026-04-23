namespace LccHotfix
{
    //本Entity被主人Entity所持有
    public class HolderEntityComponent : LogicComponent
    {
        //持有者
        public long HolderEntityID { get; set; }

        
        public LogicEntity HolderEntity
        {
            get
            {
                if (_owner == null || _owner.OwnerWorld == null)
                {
                    return null;
                }
                return _owner.OwnerWorld.GetEntityWithComID(HolderEntityID);
            }
        }
    }

    public partial class LogicEntity
    {
        public HolderEntityComponent comHolder
        {
            get { return (HolderEntityComponent)GetComponent(LogicComponentsLookup.ComHolderEntity); }
        }

        public bool hasComHolder
        {
            get { return HasComponent(LogicComponentsLookup.ComHolderEntity); }
        }

        public void AddHolderEntity(long entityID)
        {
            var index = LogicComponentsLookup.ComHolderEntity;
            var component = (HolderEntityComponent)CreateComponent(index, typeof(HolderEntityComponent));
            component.HolderEntityID = entityID;
            AddComponent(index, component);
        }

        public void ReplaceHolderEntity(long entityID)
        {
            var index = LogicComponentsLookup.ComHolderEntity;
            var component = (HolderEntityComponent)CreateComponent(index, typeof(HolderEntityComponent));
            component.HolderEntityID = entityID;
            ReplaceComponent(index, component);
        }
    }

    public static partial class LogicComponentsLookup
    {
        private static ComponentTypeIndex _ComHolderEntityIndex = new(typeof(HolderEntityComponent));
        public static int ComHolderEntity => _ComHolderEntityIndex.Index;
    }
}
