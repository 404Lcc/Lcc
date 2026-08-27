namespace LccHotfix
{
    public class SquadHeroBattleDeadComponent : LogicComponent
    {
    }

    public partial class LogicEntity
    {
        public SquadHeroBattleDeadComponent comSquadHeroBattleDead
        {
            get { return (SquadHeroBattleDeadComponent)GetComponent(LogicComponentsLookup.ComSquadHeroBattleDead); }
        }

        public bool hasComSquadHeroBattleDead
        {
            get { return HasComponent(LogicComponentsLookup.ComSquadHeroBattleDead); }
        }

        public void AddComSquadHeroBattleDead()
        {
            if (hasComSquadHeroBattleDead)
            {
                return;
            }

            var index = LogicComponentsLookup.ComSquadHeroBattleDead;
            var component = (SquadHeroBattleDeadComponent)CreateComponent(index, typeof(SquadHeroBattleDeadComponent));
            AddComponent(index, component);
        }

        public void RemoveComSquadHeroBattleDead()
        {
            if (hasComSquadHeroBattleDead)
            {
                RemoveComponent(LogicComponentsLookup.ComSquadHeroBattleDead);
            }
        }
    }

    public static partial class LogicComponentsLookup
    {
        private static ComponentTypeIndex _ComSquadHeroBattleDeadIndex = new(typeof(SquadHeroBattleDeadComponent));
        public static int ComSquadHeroBattleDead => _ComSquadHeroBattleDeadIndex.Index;
    }
}
