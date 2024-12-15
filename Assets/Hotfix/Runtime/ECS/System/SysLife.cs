using UnityEngine;
using Entitas;

namespace LccHotfix
{
    public class SysLife : IExecuteSystem
    {
        private IGroup<LogicEntity> _group;
        public SysLife(ECSWorld world)
        {
            _group = world.LogicContext.GetGroup(LogicMatcher.AllOf(LogicMatcher.ComID, LogicMatcher.ComLife));
        }

        public void Execute()
        {
            var dt = Time.deltaTime;

            var entities = _group.GetEntities();
            foreach (var entity in entities)
            {
                var comWithLife = entity.comLife;
                if (comWithLife.duration > 0)
                {
                    comWithLife.duration -= dt;

                }

                if (comWithLife.duration <= 0)
                {
                    entity.Death();
                }
            }
        }
    }
}