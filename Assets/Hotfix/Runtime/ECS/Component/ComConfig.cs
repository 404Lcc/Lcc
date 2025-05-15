using Entitas;

namespace LccHotfix
{
    public class ComConfig : LogicComponent
    {
        public int ConfigID { get; set; }
    }


    public partial class LogicEntity
    {

        public ComConfig comConfig
        {
            get { return (ComConfig)GetComponent(LogicComponentsLookup.ComConfig); }
        }

        public bool hasComConfig
        {
            get { return HasComponent(LogicComponentsLookup.ComConfig); }
        }

        public ComConfig AddComConfig(int id)
        {
            var index = LogicComponentsLookup.ComConfig;
            var component = (ComConfig)CreateComponent(index, typeof(ComConfig));
            component.ConfigID = id;
            AddComponent(index, component);
            return component;
        }

    }

    public sealed partial class LogicMatcher
    {

        static Entitas.IMatcher<LogicEntity> _matcherComConfig;

        public static Entitas.IMatcher<LogicEntity> ComConfig
        {
            get
            {
                if (_matcherComConfig == null)
                {
                    var matcher = (Entitas.Matcher<LogicEntity>)Entitas.Matcher<LogicEntity>.AllOf(LogicComponentsLookup.ComConfig);
                    matcher.ComponentNames = LogicComponentsLookup.componentNames;
                    _matcherComConfig = matcher;
                }

                return _matcherComConfig;
            }
        }
    }

    public static partial class LogicComponentsLookup
    {
        public static int ComConfig;
    }
}