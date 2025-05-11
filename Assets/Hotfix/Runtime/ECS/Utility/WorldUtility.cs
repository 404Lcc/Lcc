using Entitas;
using System.Collections.Generic;
using UnityEngine;

namespace LccHotfix
{
    public static class WorldUtility
    {
        public static LogicEntity GetEntity(long id)
        {
            return WorldManager.Instance.GetWorld().GetEntityWithComID(id);
        }

        public static LogicEntity GetEntity(GameObject go)
        {
            return WorldManager.Instance.GetWorld().GetEntitiesWithComUnityObjectRelated(go.GetInstanceID());
        }

        public static LogicEntity AddEntity<T>(GameObject obj, bool isPoolRes = false) where T : ActorView, new()
        {
            var entity = WorldManager.Instance.GetWorld().LogicContext.CreateEntity();

            entity.AddComID(IdUtility.GenerateId());

            T viewWrapper = new T();
            viewWrapper.Init(obj, isPoolRes);
            entity.AddView(viewWrapper, ViewCategory.Actor);

            entity.AddComTransform(obj.transform.position, obj.transform.rotation, obj.transform.localScale);

            var dict = new Dictionary<int, GameObjectType>();
            dict.Add(obj.GetInstanceID(), GameObjectType.Self);
            entity.AddComUnityObjectRelated(dict);
            return entity;
        }

        public static MetaContext GetMetaContext()
        {
            var metaContext = WorldManager.Instance.GetWorld().MetaContext;
            return metaContext;
        }

        public static T GetComUniGameMode<T>() where T : GameModeBase
        {
            var metaContext = GetMetaContext();
            if (metaContext.hasComUniGameMode)
            {
                return metaContext.comUniGameMode.mode as T;
            }

            return null;
        }

        public static IGroup<LogicEntity> GetLogicGroup(params IMatcher<LogicEntity>[] matchers)
        {
            var matcher = LogicMatcher.AllOf(matchers);
            var world = WorldManager.Instance.GetWorld();
            if (world == null)
                return new Group<LogicEntity>(matcher);
            var group = world.LogicContext.GetGroup(matcher);
            return group;
        }

        public static IGroup<LogicEntity> GetLogicGroup_Faction_Property_Transform()
        {
            return GetLogicGroup(LogicMatcher.AllOf(LogicMatcher.ComFaction, LogicMatcher.ComProperty, LogicMatcher.ComTransform));
        }

        public static IGroup<LogicEntity> GetLogicGroup_Subobject_Faction_Transform()
        {

            return GetLogicGroup(LogicMatcher.AllOf(LogicMatcher.ComSubobject, LogicMatcher.ComFaction, LogicMatcher.ComTransform));
        }

        public static IGroup<LogicEntity> GetLogicGroup_OwnerEntity_Tag()
        {

            return GetLogicGroup(LogicMatcher.AllOf(LogicMatcher.ComOwnerEntity, LogicMatcher.ComTag));
        }
    }
}