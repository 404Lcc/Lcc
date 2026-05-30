using System.Collections.Generic;
using Entitas;

namespace LccHotfix
{
    public class SysSyncViewHp : ReactiveSystem<LogicEntity>
    {
        public SysSyncViewHp(ECWorlds world) : base(world.LogicWorld)
        {
        }

        protected override ICollector<LogicEntity> GetTrigger(IContext<LogicEntity> context)
        {
            return new Collector<LogicEntity>(
                new IGroup<LogicEntity>[] {
                    context.GetGroup(LogicMatcher.AllOf(LogicComponentsLookup.ComHp, LogicComponentsLookup.ComView))
                },
                new GroupEvent[] {
                    GroupEvent.AddedOrRemoved,
                }
            );
        }

        protected override bool Filter(LogicEntity entity)
        {
            if (entity.hasComView && entity.hasComHp)
            {
                var hpView = entity.comView.GetView<IHpView>(EViewCategory.Hp);
                if (hpView != null)
                    return true;
            }
            return false;
        }

        protected override void Execute(List<LogicEntity> entities)
        {
            foreach (var e in entities)
            {
                var comHp = e.comHp;
                var hpView = e.comView.GetView<IHpView>(EViewCategory.Hp);
                hpView.SetHp(e.ID, comHp.Hp, comHp.MaxHp);
            }
        }
    }
}
