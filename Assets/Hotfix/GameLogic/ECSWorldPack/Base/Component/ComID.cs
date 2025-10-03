namespace LccHotfix
{
    public class ComID : LogicComponent
    {
        public long id;
    }

    public partial class LogicEntity
    {
        public ComID comID
        {
            get { return (ComID)GetComponent(LogicComponentsLookup.ComID); }
        }

        public bool hasComID
        {
            get { return HasComponent(LogicComponentsLookup.ComID); }
        }

        public void AddComID(long newId)
        {
            var index = LogicComponentsLookup.ComID;
            var component = (ComID)CreateComponent(index, typeof(ComID));
            component.id = newId;
            AddComponent(index, component);
        }
    }

    public static partial class LogicComponentsLookup
    {
        private static ComponentTypeIndex ComIDIndex = new ComponentTypeIndex(typeof(ComID));
        public static int ComID => ComIDIndex.index;
    }
}