using Entitas;
using System.Collections.Generic;
using System.Linq;

namespace LccHotfix
{
    public class BattleWorld : ECSWorld
    {
        public const string ComID = "ComID";
        public const string ComTag = "ComTag";
        public const string ComFaction = "ComFaction";
        public const string ComOwnerEntity = "ComOwnerEntity";
        public const string ComUnityObjectRelated = "ComUnityObjectRelated";

        protected override void Setup()
        {
            base.Setup();

            LogicComponentsLookup.ComID = 0;
            LogicComponentsLookup.ComTag = 1;
            LogicComponentsLookup.ComFaction = 2;
            LogicComponentsLookup.ComOwnerEntity = 3;
            LogicComponentsLookup.ComUnityObjectRelated = 4;

            LogicComponentsLookup.componentTypes.Add(typeof(ComID));
            LogicComponentsLookup.componentTypes.Add(typeof(ComTag));
            LogicComponentsLookup.componentTypes.Add(typeof(ComFaction));
            LogicComponentsLookup.componentTypes.Add(typeof(ComOwnerEntity));
            LogicComponentsLookup.componentTypes.Add(typeof(ComUnityObjectRelated));
        }
        protected override void InitializeEntityIndices()
        {
            base.InitializeEntityIndices();

            LogicContext.AddEntityIndex(new PrimaryEntityIndex<LogicEntity, long>(ComID, LogicContext.GetGroup(LogicMatcher.ComID), (e, c) => ((ComID)c).Value));
            LogicContext.AddEntityIndex(new EntityIndexEnum<LogicEntity, TagType>(ComTag, LogicContext.GetGroup(LogicMatcher.ComTag), (e, c) => ((ComTag)c).tag));
            LogicContext.AddEntityIndex(new EntityIndex<LogicEntity, FactionType>(ComFaction, LogicContext.GetGroup(LogicMatcher.ComFaction), (e, c) => ((ComFaction)c).faction));
            LogicContext.AddEntityIndex(new EntityIndex<LogicEntity, int>(ComOwnerEntity, LogicContext.GetGroup(LogicMatcher.ComOwnerEntity), (e, c) => ((ComOwnerEntity)c).OwnerEntityID));
            LogicContext.AddEntityIndex(new GroupEntityIndex<LogicEntity, int>(ComUnityObjectRelated, LogicContext.GetGroup(LogicMatcher.ComUnityObjectRelated), (e, c) => ((ComUnityObjectRelated)c).GameObjectInstanceID.Keys.ToArray()));
        }

        public LogicEntity GetEntityWithComID(long id)
        {
            return ((PrimaryEntityIndex<LogicEntity, long>)LogicContext.GetEntityIndex(ComID)).GetEntity(id);
        }

        public HashSet<LogicEntity> GetEntitiesWithComTag(TagType tag)
        {
            return ((EntityIndexEnum<LogicEntity, TagType>)LogicContext.GetEntityIndex(ComTag)).GetEntities(tag);
        }

        public HashSet<LogicEntity> GetEntitiesWithComFaction(FactionType faction)
        {
            return ((EntityIndex<LogicEntity, FactionType>)LogicContext.GetEntityIndex(ComFaction)).GetEntities(faction);
        }

        public HashSet<LogicEntity> GetEntitiesWithComOwnerEntity(int ownerEntityID)
        {
            return ((EntityIndex<LogicEntity, int>)LogicContext.GetEntityIndex(ComOwnerEntity)).GetEntities(ownerEntityID);
        }

        public LogicEntity GetEntitiesWithComUnityObjectRelated(int gameObjectInstanceID)
        {
            return ((GroupEntityIndex<LogicEntity, int>)LogicContext.GetEntityIndex(ComUnityObjectRelated)).GetEntity(gameObjectInstanceID);
        }
    }
}