using System.Collections.Generic;
using Entitas;
using UnityEngine;

namespace LccHotfix
{
    public class SysTimeScale : ReactiveSystem<MetaEntity>, ITearDownSystem
    {
        private ECWorlds _world;

        public SysTimeScale(ECWorlds world) : base(world.MetaWorld)
        {
            _world = world;
        }

        protected override ICollector<MetaEntity> GetTrigger(IContext<MetaEntity> context)
        {
            return new Collector<MetaEntity>(
                new IGroup<MetaEntity>[]
                {
                    context.GetGroup(MetaMatcher.AllOf(MetaComponentsLookup.ComUniTimeScale))
                },
                new GroupEvent[]
                {
                    GroupEvent.Added,
                }
            );
        }

        protected override bool Filter(MetaEntity entity)
        {
            return entity.HasComponent(MetaComponentsLookup.ComUniTimeScale);
        }

        protected override void Execute(List<MetaEntity> entities)
        {
            ApplyTimeScale(_world.MetaWorld.comUniTimeScale.TimeScale);
        }

        public void TearDown()
        {
            ApplyTimeScale(1f);
        }

        private void ApplyTimeScale(float timeScale)
        {
            Time.timeScale = timeScale;
        }
    }
}