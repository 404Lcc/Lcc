namespace LccHotfix
{
    public class ComUniDamage : MetaComponent
    {
        public DamageBase damage;
    }

    public partial class MetaContext
    {
        public ComUniDamage ComUniDamage
        {
            get { return GetUniqueComponent<ComUniDamage>(MetaComponentsLookup.ComUniDamage); }
        }

        public bool hasComUniDamage
        {
            get { return HasUniqueComponent(MetaComponentsLookup.ComUniDamage); }
        }

        public void SetComUniDamage(DamageBase damage)
        {
            var index = MetaComponentsLookup.ComUniDamage;
            var component = (ComUniDamage)UniqueEntity.CreateComponent(index, typeof(ComUniDamage));
            component.damage = damage;
            SetUniqueComponent(index, component);
        }
    }

    public static partial class MetaComponentsLookup
    {
        private static ComponentTypeIndex ComUniDamageIndex = new ComponentTypeIndex(typeof(ComUniDamage));
        public static int ComUniDamage => ComUniDamageIndex.index;
    }
}