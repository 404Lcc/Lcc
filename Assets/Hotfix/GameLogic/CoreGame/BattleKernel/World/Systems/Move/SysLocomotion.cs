using System.Collections.Generic;
using Entitas;

namespace LccHotfix
{
    public class SysLocomotion : IFixedUpdateSystem, IExecuteSystem
    {
        private readonly LogicWorld _logicWorld;
        private readonly MetaWorld _metaWorld;
        private readonly IGroup<LogicEntity> _group;

        private readonly List<LogicEntity> _entityBuffer = new(256);
        private int _entityBufferVersion = -1;

        public SysLocomotion(ECWorlds worlds)
        {
            _logicWorld = worlds.LogicWorld;
            _metaWorld = worlds.MetaWorld;
            _group = worlds.LogicWorld.GetGroup(LogicMatcher.AllOf(LogicComponentsLookup.ComLocomotion, LogicComponentsLookup.ComTransform));
        }
        
        private void UpdateEntities(List<LogicEntity> entities, float dt)
        {
            // B+: 允许 GST_Over 期间实体继续移动,回退恢复此 return
            // if (logicWorld.GameOver)
            //     return;

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

                if (entity.hasComAttributes)
                {
                    if (locomotion is ILocomotionSpeed locomotionSpeed
                        && !comLocomotion.SuppressAttributeSpeedSync)
                    {
                        locomotionSpeed.SetMoveSpeed(entity.GetEffectiveMoveSpeed());
                    }
                }

                locomotion.BeforeUpdate();
                var entityDt = dt * BattleBulletTimeUtility.GetCompensateRatio(entity, _metaWorld);
                locomotion.Update(entityDt, entity, _metaWorld);

                var comTransform = entity.comTransform;
                comTransform.AddPosition(locomotion.DeltaPosition);
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

        public void FixedUpdate(float dt, float dt_unscaled)
        {
            if (_metaWorld.IsInBulletTime())
            {
                return;
            }

            UpdateEntities(_group.GetEntities(_entityBuffer, ref _entityBufferVersion), dt);
        }

        public void Execute()
        {
            if (!_metaWorld.IsInBulletTime())
            {
                return;
            }

            UpdateEntities(_group.GetEntities(_entityBuffer, ref _entityBufferVersion), BattleTime.GetDeltaTime(_logicWorld));
        }
    }
}
