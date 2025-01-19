namespace LccHotfix
{
    public class ComUniDamage : MetaComponent
    {
        public DamageBase damage;
    }
    public partial class MetaContext
    {

        public MetaEntity ComUniDamageEntity { get { return GetGroup(MetaMatcher.ComUniDamage).GetSingleEntity(); } }
        public ComUniDamage ComUniDamage { get { return ComUniDamageEntity.ComUniDamage; } }
        public bool hasComUniDamage { get { return ComUniDamageEntity != null; } }

        public MetaEntity SetComUniDamage(DamageBase damage)
        {
            if (hasComUniDamage)
            {
                throw new Entitas.EntitasException("Could not set ComUniDamage!\n" + this + " already has an entity with ComUniDamage!",
                    "You should check if the context already has a ComUniDamageEntity before setting it or use context.ReplaceComUniDamage().");
            }
            var entity = CreateEntity();
            entity.AddComUniDamage(damage);
            return entity;
        }




    }
    public partial class MetaEntity
    {

        public ComUniDamage ComUniDamage { get { return (ComUniDamage)GetComponent(MetaComponentsLookup.ComUniDamage); } }
        public bool hasComUniDamage { get { return HasComponent(MetaComponentsLookup.ComUniDamage); } }

        public void AddComUniDamage(DamageBase damage)
        {
            var index = MetaComponentsLookup.ComUniDamage;
            var component = (ComUniDamage)CreateComponent(index, typeof(ComUniDamage));
            component.damage = damage;
            AddComponent(index, component);
        }

        public void ReplaceComUniDamage(DamageBase damage)
        {
            var index = MetaComponentsLookup.ComUniDamage;
            var component = (ComUniDamage)CreateComponent(index, typeof(ComUniDamage));
            component.damage = damage;
            ReplaceComponent(index, component);
        }

        public void RemoveComUniDamage()
        {
            RemoveComponent(MetaComponentsLookup.ComUniDamage);
        }
    }
    public sealed partial class MetaMatcher
    {

        static Entitas.IMatcher<MetaEntity> _matcherComUniDamage;

        public static Entitas.IMatcher<MetaEntity> ComUniDamage
        {
            get
            {
                if (_matcherComUniDamage == null)
                {
                    var matcher = (Entitas.Matcher<MetaEntity>)Entitas.Matcher<MetaEntity>.AllOf(MetaComponentsLookup.ComUniDamage);
                    matcher.ComponentNames = MetaComponentsLookup.componentNames;
                    _matcherComUniDamage = matcher;
                }

                return _matcherComUniDamage;
            }
        }
    }
    public static partial class MetaComponentsLookup
    {
        public static int ComUniDamage;
    }
}