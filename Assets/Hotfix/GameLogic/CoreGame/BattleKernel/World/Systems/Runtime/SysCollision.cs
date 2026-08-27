using System.Collections.Generic;
using Entitas;
using UnityEngine;

namespace LccHotfix
{
    public class SysCollision : IFixedUpdateSystem
    {
        private readonly IGroup<LogicEntity> group;
        private readonly MetaWorld _metaWorld;

        private readonly List<LogicEntity> _entityBuffer = new(256);

        public SysCollision(ECWorlds world)
        {
            _metaWorld = world.MetaWorld;
            group = world.LogicWorld.GetGroup(LogicMatcher.AllOf(LogicComponentsLookup.ComCollider, LogicComponentsLookup.ComTransform));
        }

        public void FixedUpdate(float dt, float dt_unscaled)
        {
            Physics.SyncTransforms();
            var buffer = group.GetEntities(_entityBuffer);
            foreach (var entity in buffer)
            {
                if (!entity.comCollider.isActive)
                {
                    continue;
                }

                var handler = entity.comCollider.handler;
                var entityDt = dt * BattleBulletTimeUtility.GetCompensateRatio(entity, _metaWorld);
                if (handler.CheckRawHits(entity, entityDt))
                {
                    handler.HandleRawHits(entity, handler.RawHitMaker.RawHits, entityDt);
                }

                handler.Cleanup();
            }
        }
    }
}
