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

    public static partial class LogicComponentsLookup
    {
        private static ComponentTypeIndex ComMonsterIndex = new ComponentTypeIndex(typeof(ComMonster));
        public static int ComMonster => ComMonsterIndex.index;
    }
}