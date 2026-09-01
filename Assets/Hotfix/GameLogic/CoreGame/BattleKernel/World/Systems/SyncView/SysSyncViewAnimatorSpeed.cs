using Entitas;
using UnityEngine;

namespace LccHotfix
{
    /// <summary>
    /// 每帧将 AnimSpeedRatio × 子弹时间补偿同步到 Animator.speed。
    /// 子弹时间只通过 GetCompensateRatio 提供倍率。
    /// </summary>
    public sealed class SysSyncViewAnimatorSpeed : IExecuteSystem
    {
        private const float DefaultRatio = 1f;

        private readonly MetaWorld _metaWorld;
        private readonly IGroup<LogicEntity> _group;

        public SysSyncViewAnimatorSpeed(ECWorlds world)
        {
            _metaWorld = world.MetaWorld;
            _group = world.LogicWorld.GetGroup(LogicMatcher.AllOf(
                LogicComponentsLookup.ComView,
                LogicComponentsLookup.ComAnimation));
        }

        public void Execute()
        {
            foreach (var entity in _group)
            {
                ApplyAnimatorSpeed(entity);
            }
        }

        private void ApplyAnimatorSpeed(LogicEntity entity)
        {
            var animator = entity.MainAnimator();
            if (animator == null)
            {
                return;
            }

            animator.speed = GetAnimSpeedRatio(entity) * BattleBulletTimeUtility.GetCompensateRatio(entity, _metaWorld);
        }

        private static float GetAnimSpeedRatio(LogicEntity entity)
        {
            if (!entity.hasComLocomotion || !entity.hasComAttributes)
            {
                return DefaultRatio;
            }

            if (entity.comAttributes.Has<float>(PropertyFloat.AnimSpeedRatio))
            {
                var animSpeedModifier = entity.comAttributes.GetAttribute<float>(PropertyFloat.AnimSpeedRatio, false);
                if (animSpeedModifier != null
                    && !Mathf.Approximately(animSpeedModifier.Value, animSpeedModifier.DefaultValue))
                {
                    return animSpeedModifier.Value;
                }
            }

            return DefaultRatio;
        }
    }
}
