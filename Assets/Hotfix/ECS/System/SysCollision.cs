using Entitas;

namespace LccHotfix
{
    public class SysCollision : IExecuteSystem
    {
        private IGroup<LogicEntity> _group;

        public SysCollision(ECSWorld world)
        {
            _group = world.LogicContext.GetGroup(LogicMatcher.AllOf(LogicMatcher.ComCollider, LogicMatcher.ComTransform));
        }

        public void Execute()
        {
            var dt = UnityEngine.Time.deltaTime;
            foreach (var entity in _group.GetEntities())
            {
                if (!entity.comCollider.isActive)
                {
                    continue;
                }
                var handler = entity.comCollider.handler;
                bool hasHits = handler.CheckRawHits(entity, dt);
                if (hasHits)
                {
                    handler.HandleRawHits(entity, dt);
                }
                handler.Cleanup();
            }
        }
    }
}