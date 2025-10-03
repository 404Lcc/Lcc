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

    public static partial class LogicComponentsLookup
    {
        private static ComponentTypeIndex ComConfigIndex = new ComponentTypeIndex(typeof(ComConfig));
        public static int ComConfig => ComConfigIndex.index;
    }
}