namespace LccHotfix
{
    public class OwnerEntityComponent : LogicComponent
    {
        public long OwnerEntityID { get; set; }
    }

    public partial class LogicEntity
    {
        public OwnerEntityComponent comOwnerEntity
        {
            get { return (OwnerEntityComponent)GetComponent(LogicComponentsLookup.ComOwnerEntity); }
        }

        public bool hasComOwnerEntity
        {
            get { return HasComponent(LogicComponentsLookup.ComOwnerEntity); }
        }

        public void AddComOwnerEntity(long newOwnerEntityID)
        {
            var index = LogicComponentsLookup.ComOwnerEntity;
            var component = (OwnerEntityComponent)CreateComponent(index, typeof(OwnerEntityComponent));
            component.OwnerEntityID = newOwnerEntityID;
            AddComponent(index, component);
        }

        public void ReplaceComOwnerEntity(long newOwnerEntityID)
        {
            var index = LogicComponentsLookup.ComOwnerEntity;
            var component = (OwnerEntityComponent)CreateComponent(index, typeof(OwnerEntityComponent));
            component.OwnerEntityID = newOwnerEntityID;
            ReplaceComponent(index, component);
        }
    }

    public static partial class LogicComponentsLookup
    {
        private static ComponentTypeIndex _ComOwnerEntityIndex = new(typeof(OwnerEntityComponent));
        public static int ComOwnerEntity => _ComOwnerEntityIndex.Index;
    }
}