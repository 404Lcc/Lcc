namespace LccHotfix
{
    public enum FactionType
    {
        None = 0,
        Player = 1,
        Enemy = 2,
    }

    public class ComFaction : LogicComponent
    {
        public FactionType faction = FactionType.None;
    }

    public partial class LogicEntity
    {
        public ComFaction comFaction { get { return (ComFaction)GetComponent(LogicComponentsLookup.ComFaction); } }
        public bool hasComFaction { get { return HasComponent(LogicComponentsLookup.ComFaction); } }

        public void AddComFaction(FactionType newFaction)
        {
            var index = LogicComponentsLookup.ComFaction;
            var component = (ComFaction)CreateComponent(index, typeof(ComFaction));
            component.faction = newFaction;
            AddComponent(index, component);
        }

        public void ReplaceComFaction(FactionType newFaction)
        {
            var index = LogicComponentsLookup.ComFaction;
            var component = (ComFaction)CreateComponent(index, typeof(ComFaction));
            component.faction = newFaction;
            ReplaceComponent(index, component);
        }

        public void RemoveComFaction()
        {
            RemoveComponent(LogicComponentsLookup.ComFaction);
        }
    }

    public sealed partial class LogicMatcher
    {
        private static Entitas.IMatcher<LogicEntity> _matcherComFaction;

        public static Entitas.IMatcher<LogicEntity> ComFaction
        {
            get
            {
                if (_matcherComFaction == null)
                {
                    var matcher = (Entitas.Matcher<LogicEntity>)Entitas.Matcher<LogicEntity>.AllOf(LogicComponentsLookup.ComFaction);
                    matcher.ComponentNames = LogicComponentsLookup.componentNames;
                    _matcherComFaction = matcher;
                }

                return _matcherComFaction;
            }
        }
    }
}