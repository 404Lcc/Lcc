using System;
using HotUpdate.Framework;
using UnityEngine;

namespace LccHotfix
{
    public sealed class BattleCollisionSpaceConfig
    {
        public AABB FullSpace { get; private set; }
        public int QuadTreeDepth { get; private set; }
        public bool EnableStatsLog { get; set; }
        public float BoundsPadding { get; set; }
        public int Version { get; private set; }

        public BattleCollisionSpaceConfig(AABB fullSpace, int quadTreeDepth, float boundsPadding = 3f)
        {
            FullSpace = fullSpace;
            QuadTreeDepth = Mathf.Max(0, quadTreeDepth);
            BoundsPadding = Mathf.Max(0f, boundsPadding);
            Version = 1;
        }
        
        public void SetFullSpace(AABB fullSpace)
        {
            if (fullSpace == null || fullSpace.IsDegenerate() || fullSpace.HasNegativeVolume())
            {
                return;
            }

            if (FullSpace != null && FullSpace.Contains(fullSpace))
            {
                return;
            }

            FullSpace = Union(FullSpace, fullSpace);
            Version++;
        }

        public void SetScreenBounds(float minX, float minY, float maxX, float maxY)
        {
            var padding = Mathf.Max(0f, BoundsPadding);
            SetFullSpace(new AABB(
                new Vector2(minX - padding, minY - padding),
                new Vector2(maxX + padding, maxY + padding)));
        }

        private static AABB Union(AABB a, AABB b)
        {
            if (a == null)
            {
                return b;
            }

            return new AABB(
                new Vector2(Mathf.Min(a.minPoint.x, b.minPoint.x), Mathf.Min(a.minPoint.y, b.minPoint.y)),
                new Vector2(Mathf.Max(a.maxPoint.x, b.maxPoint.x), Mathf.Max(a.maxPoint.y, b.maxPoint.y)));
        }
    }

    public class BattleKernelCreationInfo : IWorldCreationInfo
    {
        public BattleCollisionSpaceConfig CollisionSpaceConfig { get; set; }

        public Type MainObjectViewType { get; set; }

        public int ModeLogicID { get; set; }

        public ICustomLogicGenInfo GameModeGenInfo { get; set; }

        public IBattleModeLogicService ModeLogicService { get; set; }

        public ITimerService TimerService { get; set; }

        public IBattleTimeService BattleTimeService { get; set; }

        public IBattleFeedbackSink BattleFeedbackSink { get; set; }

        public IViewLoadService ViewLoadService { get; set; }

        public IGizmoService GizmoService { get; set; }

        public ICustomLogicService CustomLogicService { get; set; }

        public IDamagePropertyModifier DamagePropertyModifier { get; set; }

        public IDamageEventService DamageEventService { get; set; }

        public IDamagePolicyService DamagePolicyService { get; set; }

        public IUnitOwnerInfoProvider UnitOwnerInfoProvider { get; set; }

        public ICombatPropertyVolumeProvider CombatPropertyVolumeProvider { get; set; }

        public ITargetQueryService TargetQueryService { get; set; }

        public ISubobjectModelOverrideProvider SubobjectModelOverrideProvider { get; set; }

        public ISkillLogicOverrideProvider SkillLogicOverrideProvider { get; set; }

        public IBattleEffectService BattleEffectService { get; set; }

        public IBattleAudioService BattleAudioService { get; set; }

        public IBattleLogService BattleLogService { get; set; }

        public IDeathProcessService DeathProcessService { get; set; }

        public ISubobjectTransferEffectService SubobjectTransferEffectService { get; set; }
    }
}
