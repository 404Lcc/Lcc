using PBConfig;
using UnityEngine;

namespace LccHotfix
{
    public sealed class BulletTimeController
    {
        private const float MinBulletTimeScale = 0.0001f;
        public const float DefaultTransitionDuration = 0.25f;

        private float mPreBulletTimeScale = 1f;
        private bool mInBulletTime;
        private float mBulletScale = 1f;
        private float mBulletLerpFrom = 1f;
        private float mBulletLerpTo = 1f;
        private float mBulletLerpDuration;
        private float mBulletLerpElapsed;
        private bool mBulletLerping;
        private bool mBulletLeaving;

        public bool InBulletTime => mInBulletTime;
        public bool IsLerping => mBulletLerping;

        public float GetCompensateRatio(float currentTimeScale)
        {
            return mInBulletTime && currentTimeScale > MinBulletTimeScale
                ? mPreBulletTimeScale / currentTimeScale
                : 1f;
        }

        public void Enter(UniTimeScaleComponent timeScaleCom, float timeScale, float transitionDuration = -1f)
        {
            if (timeScaleCom == null)
            {
                return;
            }

            if (!mInBulletTime)
            {
                mPreBulletTimeScale = Mathf.Max(timeScaleCom.TimeScale, MinBulletTimeScale);
            }

            mInBulletTime = true;
            mBulletLeaving = false;
            var target = Mathf.Clamp(timeScale, MinBulletTimeScale, 1f);
            BeginLerp(timeScaleCom, mBulletScale, target, transitionDuration);
        }

        public void Leave(UniTimeScaleComponent timeScaleCom, float transitionDuration = -1f)
        {
            if (timeScaleCom == null || (!mInBulletTime && !mBulletLerping))
            {
                return;
            }

            mInBulletTime = true;
            mBulletLeaving = true;
            BeginLerp(timeScaleCom, mBulletScale, 1f, transitionDuration);
        }

        public void ForceStop(UniTimeScaleComponent timeScaleCom)
        {
            mBulletLerping = false;
            mBulletLeaving = false;
            mBulletScale = 1f;
            mBulletLerpElapsed = 0f;
            mInBulletTime = false;
            if (timeScaleCom != null)
            {
                timeScaleCom.ClearTimeSlowRatio(ETimeSlowFlag.SF_BulletTime);
                mPreBulletTimeScale = Mathf.Max(timeScaleCom.TimeScale, MinBulletTimeScale);
            }
            else
            {
                mPreBulletTimeScale = 1f;
            }
        }

        public bool Tick(UniTimeScaleComponent timeScaleCom, float unscaledDt)
        {
            if (!mBulletLerping || timeScaleCom == null)
            {
                return false;
            }

            if (unscaledDt < 0f)
            {
                unscaledDt = 0f;
            }

            mBulletLerpElapsed += unscaledDt;
            float t = mBulletLerpDuration <= MinBulletTimeScale
                ? 1f
                : Mathf.Clamp01(mBulletLerpElapsed / mBulletLerpDuration);
            t = t * t * (3f - 2f * t);
            ApplyBulletScale(timeScaleCom, Mathf.Lerp(mBulletLerpFrom, mBulletLerpTo, t));

            if (mBulletLerpElapsed >= mBulletLerpDuration)
            {
                FinishLerp(timeScaleCom);
            }

            return true;
        }

        private void BeginLerp(UniTimeScaleComponent timeScaleCom, float from, float to, float transitionDuration)
        {
            mBulletLerpFrom = from;
            mBulletLerpTo = to;
            mBulletLerpElapsed = 0f;

            if (transitionDuration < 0f)
            {
                transitionDuration = DefaultTransitionDuration;
            }

            if (transitionDuration <= 0f || Mathf.Abs(from - to) <= MinBulletTimeScale)
            {
                ApplyBulletScale(timeScaleCom, to);
                FinishLerp(timeScaleCom);
                return;
            }

            mBulletLerpDuration = transitionDuration;
            mBulletLerping = true;
            ApplyBulletScale(timeScaleCom, from);
        }

        private void FinishLerp(UniTimeScaleComponent timeScaleCom)
        {
            mBulletLerping = false;
            ApplyBulletScale(timeScaleCom, mBulletLerpTo);

            if (!mBulletLeaving)
            {
                return;
            }

            mBulletLeaving = false;
            timeScaleCom.ClearTimeSlowRatio(ETimeSlowFlag.SF_BulletTime);
            mBulletScale = 1f;
            mInBulletTime = false;
            mPreBulletTimeScale = Mathf.Max(timeScaleCom.TimeScale, MinBulletTimeScale);
        }

        private void ApplyBulletScale(UniTimeScaleComponent timeScaleCom, float scale)
        {
            mBulletScale = Mathf.Clamp(scale, MinBulletTimeScale, 1f);
            timeScaleCom.SetTimeSlowRatio(ETimeSlowFlag.SF_BulletTime, mBulletScale);
        }
    }

    public static class BattleBulletTimeUtility
    {
        private const float DefaultRatio = 1f;

        /// <summary>
        /// Boss 特写期间改为只补偿 EestBoss；平时仍按阵营 / ComBulletTimeCompensate。
        /// TODO: 是不是应该分不同情况维护不同的白名单列表？
        /// </summary>
        public static bool CompensateEestBossOnly { get; internal set; }

        public static float GetCompensateRatio(LogicEntity entity, MetaWorld metaWorld)
        {
            // 非子弹时间提前返回
            if (entity == null || metaWorld == null || !metaWorld.IsInBulletTime())
            {
                return DefaultRatio;
            }

            var holder = entity.hasComHolder ? entity.comHolder.HolderEntity : null;
            if (NeedsCompensate(entity) || NeedsCompensate(holder))
            {
                return metaWorld.GetBulletTimeCompensateRatio();
            }

            if (TryGetOwnRatio(entity, out var ratio))
            {
                return ratio;
            }

            return TryGetOwnRatio(holder, out ratio) ? ratio : DefaultRatio;
        }

        public static bool NeedsCompensate(LogicEntity entity)
        {
            if (entity == null)
            {
                return false;
            }

            // BossFocus期间只补偿EestBoss及其子物体
            if (CompensateEestBossOnly)
            {
                return IsEestBoss(entity);
            }

            if (entity.hasComFaction && entity.comFaction.Faction == EFaction.Friend)
            {
                return true;
            }

            // 上层挂上的补偿标记（交互物、地图物件等）
            return entity.hasComBulletTimeCompensate;
        }

        private static bool IsEestBoss(LogicEntity entity)
        {
            return entity.hasComBattleUnitTag && entity.comBattleUnitTag.Tag.EnemyStrengthType == TEnemyStrengthType.EestBoss;
        }

        public static void EnsureBulletTimeRatioAttribute(LogicEntity entity)
        {
            if (entity == null || !entity.hasComAttributes || entity.comAttributes.Has<float>(PropertyFloat.BulletTimeRatio))
            {
                return;
            }

            entity.comAttributes.SetAttribute<float, MultChangeFloat_MUL>(PropertyFloat.BulletTimeRatio, DefaultRatio);
        }

        private static bool TryGetOwnRatio(LogicEntity entity, out float ratio)
        {
            ratio = DefaultRatio;
            if (entity == null || !entity.hasComAttributes || !entity.comAttributes.Has<float>(PropertyFloat.BulletTimeRatio))
            {
                return false;
            }

            ratio = entity.GetAttributeFloat(PropertyFloat.BulletTimeRatio, DefaultRatio);
            return true;
        }
    }
}
