using Entitas;
using System.Collections.Generic;

namespace LccHotfix
{
    public class SysSyncViewAnimator : ReactiveSystem<LogicEntity>
    {
        public SysSyncViewAnimator(ECWorlds worlds) : base(worlds.LogicWorld)
        {
        }

        protected override ICollector<LogicEntity> GetTrigger(IContext<LogicEntity> context)
        {
            return new Collector<LogicEntity>(
                new IGroup<LogicEntity>[]
                {
                    context.GetGroup(LogicMatcher.AllOf(LogicComponentsLookup.ComView, LogicComponentsLookup.ComAnimation))
                },
                new GroupEvent[]
                {
                    GroupEvent.AddedOrRemoved,
                }
            );
        }

        protected override bool Filter(LogicEntity entity)
        {
            return entity.hasComAnimation;
        }

        protected override void Execute(List<LogicEntity> entities)
        {
            foreach (var entity in entities)
            {
                var comAnimation = entity.comAnimation;

                if (entity.hasComView)
                {
                    comAnimation.Ctrl?.SyncData(entity.comView, ref comAnimation.Data);
                }
                else
                {
                    comAnimation.Ctrl?.SyncData(null, ref comAnimation.Data);
                }
            }
        }
    }
}