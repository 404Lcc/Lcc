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
                return metaContext.ComUniGameMode.mode as T;
            }

            return null;
        }

        public static IGroup<LogicEntity> GetLogicGroup(params int[] indices)
        {
            var matcher = LogicMatcher.AllOf(indices);
            var world = Main.WorldService.GetWorld();
            if (world == null)
                return new Group<LogicEntity>(matcher);
            var group = world.LogicContext.GetGroup(matcher);
            return group;
        }

        public static IGroup<LogicEntity> GetLogicGroup_Faction_Property_Transform()
        {
            return GetLogicGroup(LogicComponentsLookup.ComFaction, LogicComponentsLookup.ComProperty, LogicComponentsLookup.ComTransform);
        }

        public static IGroup<LogicEntity> GetLogicGroup_Subobject_Faction_Transform()
        {

            return GetLogicGroup(LogicComponentsLookup.ComSubobject, LogicComponentsLookup.ComFaction, LogicComponentsLookup.ComTransform);
        }

        public static IGroup<LogicEntity> GetLogicGroup_OwnerEntity_Tag()
        {

            return GetLogicGroup(LogicComponentsLookup.ComOwnerEntity, LogicComponentsLookup.ComTag);
        }
    }
}