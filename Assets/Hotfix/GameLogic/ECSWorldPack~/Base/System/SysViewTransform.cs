using UnityEngine;
using Entitas;
using System.Collections.Generic;

namespace LccHotfix
{
    public class SysSyncViewTransform : ReactiveSystem<LogicEntity>
    {

        public SysSyncViewTransform(ECSWorld world) : base(world.LogicContext)
        {
        }

        protected override ICollector<LogicEntity> GetTrigger(IContext<LogicEntity> context)
        {
            return new Collector<LogicEntity>(
                new IGroup<LogicEntity>[]
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
                var position = entity.comTransform.position;
                var rotation = entity.comTransform.rotation;
                var scale = entity.comTransform.scale;

                if (float.IsNaN(position.x) || float.IsNaN(position.y) || float.IsNaN(position.z))
                {
                    continue;
                }

                var comView = entity.comView;
                var viewList = comView.ViewList;
                for (int i = 0; i < viewList.Count; i++)
                {
                    var viewWrapper = viewList[i];
                    viewWrapper.SyncTransform(position, rotation, new Vector3(scale.x * entity.comTransform.dirX, scale.y, scale.z));
                }
            }
        }
    }
}