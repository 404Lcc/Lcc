namespace LccHotfix
{
    public class ComHP : LogicComponent
    {
        private float _hp;
        public float HP => _hp;

        public void SetHP(float newHP)
        {
            _hp = newHP;
        }

        public void ChangeHP(float changeHP)
        {
            _hp += changeHP;
        }
    }

    public partial class LogicEntity
    {
        public ComHP comHP
        {
            get { return (ComHP)GetComponent(LogicComponentsLookup.ComHP); }
        }

        public bool hasComHP
        {
            get { return HasComponent(LogicComponentsLookup.ComHP); }
        }

        public void AddComHP(float newHP)
        {
            var index = LogicComponentsLookup.ComHP;
            var component = (ComHP)CreateComponent(index, typeof(ComHP));
            AddComponent(index, component);
            component.SetHP(newHP);
        }

        public void RemoveComHP()
        {
            RemoveComponent(LogicComponentsLookup.ComHP);
        }
    }

    public static partial class LogicComponentsLookup
    {
        private static ComponentTypeIndex ComHPIndex = new ComponentTypeIndex(typeof(ComHP));
        public static int ComHP => ComHPIndex.index;
    }
}