using Entitas;

namespace LccHotfix
{
    public class ComHero : LogicComponent
    {
        public int ConfigID { get; set; }
    }


    public partial class LogicEntity
    {

        public ComHero comHero
        {
            get { return (ComHero)GetComponent(LogicComponentsLookup.ComHero); }
        }

        public bool hasComHero
        {
            get { return HasComponent(LogicComponentsLookup.ComHero); }
        }

        public ComHero AddComHero()
        {
            var index = LogicComponentsLookup.ComHero;
            var component = (ComHero)CreateComponent(index, typeof(ComHero));
            AddComponent(index, component);
            return component;
        }
        
        public void RemoveComHero()
        {
            RemoveComponent(LogicComponentsLookup.ComHero);
        }

    }

    public sealed partial class LogicMatcher
    {

        static Entitas.IMatcher<LogicEntity> _matcherComHero;

        public static Entitas.IMatcher<LogicEntity> ComHero
        {
            get
            {
                if (_matcherComHero == null)
                {
                    var matcher = (Entitas.Matcher<LogicEntity>)Entitas.Matcher<LogicEntity>.AllOf(LogicComponentsLookup.ComHero);
                    matcher.ComponentNames = LogicComponentsLookup.componentNames;
                    _matcherComHero = matcher;
                }

                return _matcherComHero;
            }
        }
    }

    public static partial class LogicComponentsLookup
    {
        public static int ComHero;
    }
}