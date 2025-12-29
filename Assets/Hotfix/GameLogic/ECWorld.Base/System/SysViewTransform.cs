using UnityEngine;
using Entitas;
using System.Collections.Generic;

namespace LccHotfix
{
    public class SysSyncViewTransform : ReactiveSystem<LogicEntity>
    {
        public SysSyncViewTransform(LogicWorld logicWorld) : base(logicWorld)
        {
        }

        protected override ICollector<LogicEntity> GetTrigger(IContext<LogicEntity> context)
        {
            return new Collector<LogicEntity>(new IGroup<LogicEntity>[]
                {
                    context.GetGroup(LogicMatcher.AllOf(LogicComponentsLookup.ComView, LogicComponentsLookup.ComTransform))
                },
                new GroupEvent[]
                {
                    GroupEvent.AddedOrRemoved,
                }
            );
        }

        protected override bool Filter(LogicEntity entity)
        {
            return entity.hasComView && entity.hasComTransform;
        }

        protected override void Execute(List<LogicEntity> entities)
        {
            foreach (var entity in entities)
            {
                var position = entity.position;
                var rotation = entity.rotation;
                var scale = entity.scale;

                if (float.IsNaN(position.x))
                {
                    Debug.LogError($"SysSyncViewTransform: entityID = {entity.ID} position X is NaN");
                    continue;
                }

                if (float.IsNaN(position.z))
                {
                    Debug.LogError($"SysSyncViewTransform: entityID = {entity.ID} position Z is NaN");
                    continue;
                }

                var comView = entity.comView;
                var viewList = comView.ViewList;
                for (int i = 0; i < viewList.Count; i++)
                {
                    var viewWrapper = viewList[i];
                    viewWrapper.SyncTransform(entity.ID, position, rotation, new Vector3(scale.x, scale.y, scale.z));
                }
            }
        }
    }
}