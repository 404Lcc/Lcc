using UnityEngine;
using Entitas;

namespace LccHotfix
{
    public class SysLocomotion : IExecuteSystem
    {
        private ECSWorld _contexts;
        private IGroup<LogicEntity> _group;
        public SysLocomotion(ECSWorld contexts)
        {
            _contexts = contexts;
            _group = _contexts.LogicContext.GetGroup(LogicMatcher.AllOf(LogicMatcher.ComLocomotion, LogicMatcher.ComTransform));
        }

        public void Execute()
        {
            float dt = Time.deltaTime;
            var entities = _group.GetEntities();
            foreach (var entity in entities)
            {
                var comTransform = entity.comTransform;
                var comLocomotion = entity.comLocomotion;
                var locomotion = comLocomotion.Locomotion;

                if (locomotion.IsEnd())
                {
                    entity.RemoveComLocomotion();
                    continue;
                }

                if (!CheckIsMovable(entity))
                {
                    continue;
                }

                locomotion.Update(dt);
                comTransform.SetPosition(locomotion.CurPosition);
                comTransform.SetRotation(locomotion.CurRotation);
                comTransform.SetScale(locomotion.CurScale);
            }
        }



        public virtual bool CheckIsMovable(LogicEntity entity)
        {
            return true;
        }
    }
}