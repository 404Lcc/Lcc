namespace LccHotfix
{
    public class ComOwnerEntity : LogicComponent
    {
        public long ownerEntityID;
    }

    public partial class LogicEntity
    {
        public ComOwnerEntity comOwnerEntity
        {
            get { return (ComOwnerEntity)GetComponent(LogicComponentsLookup.ComOwnerEntity); }
        }

        public bool hasComOwnerEntity
        {
            get { return HasComponent(LogicComponentsLookup.ComOwnerEntity); }
        }

        public void AddComOwnerEntity(long newOwnerEntityID)
        {
            var index = LogicComponentsLookup.ComOwnerEntity;
            var component = (ComOwnerEntity)CreateComponent(index, typeof(ComOwnerEntity));
            component.ownerEntityID = newOwnerEntityID;
            AddComponent(index, component);
        }

        public void ReplaceComOwnerEntity(long newOwnerEntityID)
        {
            var index = LogicComponentsLookup.ComOwnerEntity;
            var component = (ComOwnerEntity)CreateComponent(index, typeof(ComOwnerEntity));
            component.ownerEntityID = newOwnerEntityID;
            ReplaceComponent(index, component);
        }
    }

    public static partial class LogicComponentsLookup
    {
        private static ComponentTypeIndex ComOwnerEntityIndex = new ComponentTypeIndex(typeof(ComOwnerEntity));
        public static int ComOwnerEntity => ComOwnerEntityIndex.index;
    }
}