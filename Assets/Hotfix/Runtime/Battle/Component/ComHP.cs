namespace LccHotfix
{
    public class ComHP : LogicComponent
    {
        private float _hp;
        public float HP => _hp;

        public void SetHP(float newHp)
        {
            _hp = newHp;
        }

        public void ChangeHP(float changeHp)
        {
            _hp += changeHp;
        }
    }

    public partial class LogicEntity
    {
        public ComHP comHP { get { return (ComHP)GetComponent(LogicComponentsLookup.ComHP); } }
        public bool hasComHP { get { return HasComponent(LogicComponentsLookup.ComHP); } }

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

    public sealed partial class LogicMatcher
    {
        private static Entitas.IMatcher<LogicEntity> _matcherComHp;

        public static Entitas.IMatcher<LogicEntity> ComHp
        {
            get
            {
                if (_matcherComHp == null)
                {
                    var matcher = (Entitas.Matcher<LogicEntity>)Entitas.Matcher<LogicEntity>.AllOf(LogicComponentsLookup.ComHP);
                    matcher.ComponentNames = LogicComponentsLookup.componentNames;
                    _matcherComHp = matcher;
                }

                return _matcherComHp;
            }
        }
    }
}