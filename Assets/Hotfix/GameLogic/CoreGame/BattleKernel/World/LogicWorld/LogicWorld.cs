using Entitas;
using System.Collections.Generic;

namespace LccHotfix
{
    public partial class LogicWorld
    {
        private IGroup<LogicEntity> _group_Bounds_NoneSubobject;
        private IGroup<LogicEntity> _group_Faction_HP_Transform;
        private IGroup<LogicEntity> _group_BattleUnitTag;

        public bool GameOver = false;

        protected IGroup<LogicEntity> GetLogicGroupAllOf(params int[] indices)
        {
            var matcher = LogicMatcher.AllOf(indices);
            var group = GetGroup(matcher);
            return group;
        }

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
                _group_Faction_HP_Transform = GetLogicGroupAllOf(LogicComponentsLookup.ComFaction, LogicComponentsLookup.ComHp, LogicComponentsLookup.ComTransform);
            }

            return _group_Faction_HP_Transform;
        }

        public IGroup<LogicEntity> GetLogicGroup_BattleUnitTag()
        {
            if (_group_BattleUnitTag == null)
            {
                _group_BattleUnitTag = GetLogicGroupAllOf(LogicComponentsLookup.ComBattleUnitTag);
            }

            return _group_BattleUnitTag;
        }


        public HashSet<LogicEntity> GetEntitiesWithComFaction(EFaction faction)
        {
            return GetEntityIndex<FactionComponent, EntityIndex<LogicEntity, EFaction>>().GetEntities(faction);
        }

        public HashSet<LogicEntity> GetEntitiesWithComOwnerEntity(long ownerEntityID)
        {
            return GetEntityIndex<HolderEntityComponent, EntityIndex<LogicEntity, long>>().GetEntities(ownerEntityID);
        }

        public LogicEntity GetEntityWithComID(long id)
        {
            var index = GetEntityIndex(EntityIndexName.IDComponent) as PrimaryEntityIndex<LogicEntity, long>;
            if (index == null)
            {
                return null;
            }

            return index.GetEntity(id);
        }


        public HashSet<LogicEntity> GetEntitiesWithComBattleUnitTid(int unitTid)
        {
            var index = GetEntityIndex(EntityIndexName.BattleUnitTagComponent) as EntityIndex<LogicEntity, int>;
            if (index == null)
            {
                return null;
            }

            return index.GetEntities(unitTid);
        }

        public LogicEntity GetEntitiesWithComUnityObjectRelated(int gameObjectInstanceID)
        {
            var index = GetEntityIndex(EntityIndexName.UnityObjectRelatedComponent) as GroupEntityIndex<LogicEntity, int>;
            if (index == null)
            {
                return null;
            }

            return index.GetEntity(gameObjectInstanceID);
        }
    }
}
