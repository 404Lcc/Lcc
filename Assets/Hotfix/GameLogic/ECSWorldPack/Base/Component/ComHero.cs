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

    public static partial class LogicComponentsLookup
    {
        private static ComponentTypeIndex ComHeroIndex = new ComponentTypeIndex(typeof(ComHero));
        public static int ComHero => ComHeroIndex.index;
    }
}