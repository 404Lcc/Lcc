using Entitas;
using System.Collections.Generic;

namespace LccHotfix
{
    public class SysLocomotion : SysGroupTickBase<LogicEntity>
    {
        public SysLocomotion(ECWorlds worlds) : base(worlds)
        {
        }

        protected override IGroup<LogicEntity> InnerGetGroup()
        {
            return _worlds.LogicWorld.GetGroup(LogicMatcher.AllOf(LogicComponentsLookup.ComLocomotion, LogicComponentsLookup.ComTransform));
        }

        protected override void UpdateEntities(List<LogicEntity> entities, float dt)
        {
            if (_worlds.LogicWorld.GameOver)
                return;
            
            foreach (var entity in entities)
            {
                var comLocomotion = entity.comLocomotion;
                var locomotion = comLocomotion.Locomotion;

                if (locomotion.IsEnd())
                {
                    entity.RemoveComLocomotion();
                    if (entity.hasComCommandSender)
                    {
                        var cmd = new EntityCommand()
                        {
                            CmdType = EntityCmdType.Nt_OnLocomotionEnd,
                            EntityID = entity.ID,
                        };
                        entity.SendCmd(cmd);
                    }

                    continue;
                }

                if (!CheckIsMovable(entity))
                {
                    continue;
                }

                var moveSpeedRatio = 1f;
                if (entity.hasComAttributes)
                {
                    if (locomotion is ILocomotionSpeed locomotionSpeed)
                    {
                        locomotionSpeed.SetMoveSpeed(entity.GetAttributeFloat(PropertyFloat.MoveSpeed));
                    }

                    if (entity.comAttributes.Has<float>(PropertyFloat.MoveSpeedRatio))
                    {
                        moveSpeedRatio = entity.GetAttributeFloat(PropertyFloat.MoveSpeedRatio);
                    }
                }

                locomotion.BeforeUpdate();
                locomotion.Update(dt, entity);

                var comTransform = entity.comTransform;
                comTransform.AddPosition(locomotion.DeltaPosition * moveSpeedRatio);
                comTransform.AddRotation(locomotion.DeltaRotation);
            }
        }

        protected override void LateUpdateEntities(List<LogicEntity> entities, float dt)
        {
            foreach (var entity in entities)
            {
                var comLocomotion = entity.comLocomotion;
                var locomotion = comLocomotion.Locomotion;

                if (!CheckIsMovable(entity))
                {
                    continue;
                }

                locomotion.LateUpdate(dt, entity);
            }
        }

        public virtual bool CheckIsMovable(LogicEntity entity)
        {
            if (!entity.hasComAttributes)
                return true;
            if (!entity.GetAttributeBool(AttributeBool.Moveable))
            {
                return false;
            }

            return true;
        }
    }
}
