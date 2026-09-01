using Entitas;

namespace LccHotfix
{
    public partial class LogicWorld
    {
        private IGroup<LogicEntity> _group_Bounds_NoneSubobject;
        private IGroup<LogicEntity> _group_Faction_HP_Transform;
        private IGroup<LogicEntity> _group_BattleUnitTag;
        private IGroup<LogicEntity> _group_Hero;
        private IGroup<LogicEntity> _group_Monster;
        
        public IGroup<LogicEntity> GetLogicGroup_Bounds_NoneSubobject()
        {
            if (_group_Bounds_NoneSubobject == null)
            {
                _group_Bounds_NoneSubobject = GetGroup(LogicMatcher.AllOf(LogicComponentsLookup.ComBounds).NoneOf(LogicComponentsLookup.ComSubobject));
            }

            return _group_Bounds_NoneSubobject;
        }

        public IGroup<LogicEntity> GetLogicGroup_Faction_HP_Transform()
        {
            if (_group_Faction_HP_Transform == null)
            {
                _group_Faction_HP_Transform = GetGroup(LogicMatcher.AllOf(LogicComponentsLookup.ComFaction, LogicComponentsLookup.ComHp, LogicComponentsLookup.ComTransform));
            }

            return _group_Faction_HP_Transform;
        }

        public IGroup<LogicEntity> GetLogicGroup_BattleUnitTag()
        {
            if (_group_BattleUnitTag == null)
            {
                _group_BattleUnitTag = GetGroup(LogicMatcher.AllOf(LogicComponentsLookup.ComBattleUnitTag));
            }

            return _group_BattleUnitTag;
        }
        
        public LogicEntity GetLogicGroup_Hero()
        {
            if (_group_Hero == null)
            {
                _group_Hero = GetGroup(LogicMatcher.AllOf(LogicComponentsLookup.ComHero));
            }

            return _group_Hero.GetSingleEntity();
        }
        
        public IGroup<LogicEntity> GetLogicGroup_Monster()
        {
            if (_group_Monster == null)
            {
                _group_Monster = GetGroup(LogicMatcher.AllOf(LogicComponentsLookup.ComMonster));
            }

            return _group_Monster;
        }
    }
}
