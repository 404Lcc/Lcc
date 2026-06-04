using Entitas;
using UnityEngine;

namespace LccHotfix
{
    public class SysLocomotion : IExecuteSystem
    {
        private readonly ECWorlds _worlds;
        private readonly IGroup<LogicEntity> _group;

        public SysLocomotion(ECWorlds worlds)
        {
            _worlds = worlds;
            _group = worlds.LogicWorld.GetGroup(LogicMatcher.AllOf(LogicComponentsLookup.ComLocomotion, LogicComponentsLookup.ComTransform));
        }

        public void Execute()
        {
            UpdateEntities(_group.GetEntities(), BattleTime.GetDeltaTime(_worlds.LogicWorld));
        }

        private void UpdateEntities(LogicEntity[] entities, float dt)
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
