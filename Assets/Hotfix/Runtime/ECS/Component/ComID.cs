namespace LccHotfix
{
    public class ComID : LogicComponent
    {
        public long Value;
    }

    public partial class LogicEntity
    {
        public ComID comID { get { return (ComID)GetComponent(LogicComponentsLookup.ComID); } }
        public bool hasComID { get { return HasComponent(LogicComponentsLookup.ComID); } }

        public void AddComID(long newValue)
        {
            var index = LogicComponentsLookup.ComID;
            var component = (ComID)CreateComponent(index, typeof(ComID));
            component.Value = newValue;
            AddComponent(index, component);
        }
    }

    public sealed partial class LogicMatcher
    {
        private static Entitas.IMatcher<LogicEntity> _matcherComID;

        public static Entitas.IMatcher<LogicEntity> ComID
        {
            get
            {
                if (_matcherComID == null)
                {
                    var matcher = (Entitas.Matcher<LogicEntity>)Entitas.Matcher<LogicEntity>.AllOf(LogicComponentsLookup.ComID);
                    matcher.ComponentNames = LogicComponentsLookup.componentNames;
                    _matcherComID = matcher;
                }

                return _matcherComID;
            }
        }
    }
}