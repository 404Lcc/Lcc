using Entitas;
using System.Collections.Generic;
using UnityEngine;

namespace LccHotfix
{
    public static class WorldUtility
    {
        public static MetaContext GetMetaContext()
        {
            var metaContext = Main.WorldService.GetWorld().MetaContext;
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
            var world = Main.WorldService.GetWorld();
            if (world == null)
                return new Group<LogicEntity>(matcher);
            var group = world.LogicContext.GetGroup(matcher);
            return group;
        }

        public static IGroup<LogicEntity> GetLogicGroup_Faction_Property_Transform()
        {
            return GetLogicGroup(LogicMatcher.ComFaction, LogicMatcher.ComProperty, LogicMatcher.ComTransform);
        }

        public static IGroup<LogicEntity> GetLogicGroup_Subobject_Faction_Transform()
        {

            return GetLogicGroup(LogicMatcher.ComSubobject, LogicMatcher.ComFaction, LogicMatcher.ComTransform);
        }

        public static IGroup<LogicEntity> GetLogicGroup_OwnerEntity_Tag()
        {

            return GetLogicGroup(LogicMatcher.ComOwnerEntity, LogicMatcher.ComTag);
        }
    }
}