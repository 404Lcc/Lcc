using System;
using HotUpdate.Framework;

namespace LccHotfix
{
    public class BattleKernelCreationInfo : IWorldCreationInfo
    {
        public Type MainObjectViewType { get; set; }

        public int ModeLogicID { get; set; }

        public ICustomLogicGenInfo GameModeGenInfo { get; set; }

        public IBattleModeLogicService ModeLogicService { get; set; }

        public ITimerService TimerService { get; set; }

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