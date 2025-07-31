namespace LccHotfix
{
    public class ComOwnerEntity : LogicComponent
    {
        public long ownerEntityID;
    }

    public partial class LogicEntity
    {
        public ComOwnerEntity comOwnerEntity { get { return (ComOwnerEntity)GetComponent(LogicComponentsLookup.ComOwnerEntity); } }
        public bool hasComOwnerEntity { get { return HasComponent(LogicComponentsLookup.ComOwnerEntity); } }

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

    public sealed partial class LogicMatcher
    {
        private static Entitas.IMatcher<LogicEntity> _matcherComOwnerEntity;

        public static Entitas.IMatcher<LogicEntity> ComOwnerEntity
        {
            get
            {
                if (_matcherComOwnerEntity == null)
                {
                    var matcher = (Entitas.Matcher<LogicEntity>)Entitas.Matcher<LogicEntity>.AllOf(LogicComponentsLookup.ComOwnerEntity);
                    matcher.ComponentNames = LogicComponentsLookup.componentNames;
                    _matcherComOwnerEntity = matcher;
                }

                return _matcherComOwnerEntity;
            }
        }
    }
    public static partial class LogicComponentsLookup
    {
        public static int ComOwnerEntity;
    }
}