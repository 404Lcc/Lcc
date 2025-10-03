using System.Collections;
using System.Collections.Generic;
using Entitas;
using UnityEngine;

namespace LccHotfix
{
    public class SysAABBCollision : IExecuteSystem
    {
        private IGroup<LogicEntity> _group;

        public SysAABBCollision(ECSWorld world)
        {
            _group = world.LogicContext.GetGroup(LogicMatcher.AllOf(LogicMatcher.ComAABBCollider, LogicMatcher.ComTransform));
        }


        public void Execute()
        {
            foreach (var entity in _group.GetEntities())
            {
                var comAABBCollider = entity.ComAABBCollider;
                var handle = comAABBCollider.handler;
                if (!comAABBCollider.isActive)
                    continue;

                if (handle == null)
                    continue;
                handle.CheckHits(entity);
            }
        }
    }
}