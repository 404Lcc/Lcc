using System.Collections.Generic;
using Entitas;

namespace LccHotfix
{
    /// <summary>
    /// LogicWorld 无状态索引查询扩展。
    /// </summary>
    public static class LogicWorldQueryExtensions
    {
        public static HashSet<LogicEntity> GetEntitiesWithComFaction(this LogicWorld world, EFaction faction)
        {
            var index = world.GetEntityIndex(EntityIndexName.FactionComponent) as EntityIndex<LogicEntity, EFaction>;
            if (index == null)
            {
                return null;
            }

            return index.GetEntities(faction);
        }

        /// <summary>
        /// 按阵营取可战斗单位（Faction+Hp+View+Transform、非 Death）。
        /// </summary>
        public static HashSet<LogicEntity> GetFactionFighters(this LogicWorld world, EFaction faction)
        {
            var index = world.GetEntityIndex(EntityIndexName.FactionFighter) as EntityIndex<LogicEntity, EFaction>;
            if (index == null)
            {
                return null;
            }

            return index.GetEntities(faction);
        }

        public static LogicEntity GetEntityWithComID(this LogicWorld world, long id)
        {
            if (id <= 0)
            {
                return null;
            }

            var index = world.GetEntityIndex(EntityIndexName.IDComponent) as PrimaryEntityIndex<LogicEntity, long>;
            if (index == null)
            {
                return null;
            }

            return index.GetEntity(id);
        }

        public static HashSet<LogicEntity> GetEntitiesWithComBattleUnitTid(this LogicWorld world, int unitTid)
        {
            var index = world.GetEntityIndex(EntityIndexName.BattleUnitTagComponent) as EntityIndex<LogicEntity, int>;
            if (index == null)
            {
                return null;
            }

            return index.GetEntities(unitTid);
        }

        public static LogicEntity GetEntitiesWithComUnityObjectRelated(this LogicWorld world, int gameObjectInstanceID)
        {
            var index = world.GetEntityIndex(EntityIndexName.UnityObjectRelatedComponent) as GroupEntityIndex<LogicEntity, int>;
            if (index == null)
            {
                return null;
            }

            return index.GetEntity(gameObjectInstanceID);
        }
    }
}
