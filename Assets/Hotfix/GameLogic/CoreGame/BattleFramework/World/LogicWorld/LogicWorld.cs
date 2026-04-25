using Entitas;
using System.Collections.Generic;

namespace LccHotfix
{
    public partial class LogicWorld
    {
        protected IGroup<LogicEntity> GetLogicGroupAllOf(params int[] indices)
        {
            var matcher = LogicMatcher.AllOf(indices);
            var group = GetGroup(matcher);
            return group;
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