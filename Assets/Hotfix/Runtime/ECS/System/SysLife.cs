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
            float dt = Time.deltaTime;
            foreach (var entity in _group.GetEntities())
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