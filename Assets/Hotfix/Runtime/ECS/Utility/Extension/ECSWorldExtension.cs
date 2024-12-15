using Entitas;
using System.Collections.Generic;

namespace LccHotfix
{
    public static class ECSWorldExtension
    {
        public static LogicEntity GetEntityWithComID(this ECSWorld world, long id)
        {
            return world.GetEntityIndex<ComID, PrimaryEntityIndex<LogicEntity, long>>().GetEntity(id);
        }

        public static HashSet<LogicEntity> GetEntitiesWithComTag(this ECSWorld world, TagType tag)
        {
            return world.GetEntityIndex<ComTag, EntityIndexEnum<LogicEntity, TagType>>().GetEntities(tag);
        }

        public static HashSet<LogicEntity> GetEntitiesWithComFaction(this ECSWorld world, FactionType faction)
        {
            return world.GetEntityIndex<ComFaction, EntityIndex<LogicEntity, FactionType>>().GetEntities(faction);
        }

        public static HashSet<LogicEntity> GetEntitiesWithComOwnerEntity(this ECSWorld world, int ownerEntityID)
        {
            return world.GetEntityIndex<ComOwnerEntity, EntityIndex<LogicEntity, int>>().GetEntities(ownerEntityID);
        }

        public static LogicEntity GetEntitiesWithComUnityObjectRelated(this ECSWorld world, int gameObjectInstanceID)
        {
            return world.GetEntityIndex<ComUnityObjectRelated, GroupEntityIndex<LogicEntity, int>>().GetEntity(gameObjectInstanceID);
        }
    }
}