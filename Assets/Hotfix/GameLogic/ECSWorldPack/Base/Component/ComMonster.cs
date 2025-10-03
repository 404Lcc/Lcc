namespace LccHotfix
{
    public class ComMonster : LogicComponent
    {
        public int ConfigID { get; set; }
    }

    public partial class LogicEntity
    {

        public ComMonster comMonster
        {
            get { return (ComMonster)GetComponent(LogicComponentsLookup.ComMonster); }
        }

        public bool hasComMonster
        {
            get { return HasComponent(LogicComponentsLookup.ComMonster); }
        }

        public ComMonster AddComMonster()
        {
            var index = LogicComponentsLookup.ComMonster;
            var component = (ComMonster)CreateComponent(index, typeof(ComMonster));
            AddComponent(index, component);
            return component;
        }
        
        public void RemoveComMonster()
        {
            RemoveComponent(LogicComponentsLookup.ComMonster);
        }

    }

    public sealed partial class LogicMatcher
    {

        static Entitas.IMatcher<LogicEntity> _matcherComMonster;

        public static Entitas.IMatcher<LogicEntity> ComMonster
        {
            get
            {
                if (_matcherComMonster == null)
                {
                    var matcher = (Entitas.Matcher<LogicEntity>)Entitas.Matcher<LogicEntity>.AllOf(LogicComponentsLookup.ComMonster);
                    matcher.ComponentNames = LogicComponentsLookup.componentNames;
                    _matcherComMonster = matcher;
                }

                return _matcherComMonster;
            }
        }
    }

    public static partial class LogicComponentsLookup
    {
        public static int ComMonster;
    }
}