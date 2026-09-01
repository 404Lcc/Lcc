using System.Collections.Generic;
using Entitas;
using UnityEngine;

namespace LccHotfix
{
    public sealed class SysBulletTime : IExecuteSystem, ITearDownSystem
    {
        private readonly LogicWorld _logicWorld;
        private readonly MetaWorld _metaWorld;
        private readonly IGroup<LogicEntity> _group;

        private readonly List<LogicEntity> _entityBuffer = new(256);
        private int _entityBufferVersion = -1;

        private const int BulletTimeModifyFlag = (int)ETimeSlowFlag.SF_BulletTime;

        public SysBulletTime(ECWorlds world)
        {
            _logicWorld = world.LogicWorld;
            _metaWorld = world.MetaWorld;
            _group = _logicWorld.GetGroup(LogicMatcher.AnyOf(
                LogicComponentsLookup.ComFaction,
                LogicComponentsLookup.ComBulletTimeCompensate));
        }

        public void Execute()
        {
            _metaWorld.TickBulletTime(Time.unscaledDeltaTime);

            var inBulletTime = _metaWorld.IsInBulletTime();
            var ratio = inBulletTime ? _metaWorld.GetBulletTimeCompensateRatio() : 1f;
            var buffer = _group.GetEntities(_entityBuffer, ref _entityBufferVersion);
            foreach (var entity in buffer)
            {
                if (!BattleBulletTimeUtility.NeedsCompensate(entity))
                {
                    ClearRatio(entity);
                    continue;
                }

                if (entity.hasComAttributes)
                {
                    BattleBulletTimeUtility.EnsureBulletTimeRatioAttribute(entity);
                    if (inBulletTime)
                    {
                        entity.ModifyAttribute(PropertyFloat.BulletTimeRatio, ratio, BulletTimeModifyFlag);
                    }
                    else
                    {
                        ClearRatio(entity);
                    }
                }
            }
        }

        public void TearDown()
        {
            _metaWorld.ForceStopBulletTime();
            var buffer = _group.GetEntities(_entityBuffer, ref _entityBufferVersion);
            foreach (var entity in buffer)
            {
                ClearRatio(entity);
            }
        }

        private static void ClearRatio(LogicEntity entity)
        {
            if (entity == null || !entity.hasComAttributes || !entity.comAttributes.Has<float>(PropertyFloat.BulletTimeRatio))
            {
                return;
            }

            entity.RemoveModify<float>(PropertyFloat.BulletTimeRatio, BulletTimeModifyFlag);
        }
    }
}
