using UnityEngine;
using Entitas;

namespace LccHotfix
{
    public class SysLocomotion : IExecuteSystem
    {
        private IGroup<LogicEntity> _group;

        public SysLocomotion(ECSWorld world)
        {
            _group = world.LogicContext.GetGroup(LogicMatcher.AllOf(LogicComponentsLookup.ComLocomotion, LogicComponentsLookup.ComTransform));
        }

        public void Execute()
        {
            float dt = Time.deltaTime;
            foreach (var entity in _group.GetEntities())
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