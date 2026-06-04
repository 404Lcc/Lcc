using System;

namespace LccHotfix
{
    public class DemoWorldCreationInfo : BattleKernelCreationInfo
    {
        public const int DemoModeLogicId = 1;

        public DemoWorldCreationInfo()
        {
            MainObjectViewType = typeof(MainGameObjectView);
            ModeLogicID = DemoModeLogicId;
            GameModeGenInfo = new ICustomLogicGenInfo
            {
                LogicConfigID = ModeLogicID,
                ConfigContainerName = LogicContainerKey.LogicConfigs_GameMode
            };
            TimerService = Main.TimerService;
            BattleFeedbackSink = new DemoBattleFeedbackSink();
            ViewLoadService = new DemoViewLoadService();
            GizmoService = Main.GizmoService;
            CustomLogicService = Main.CustomLogicService;
            DamagePropertyModifier = new DemoDamagePropertyModifier();
            DamageEventService = new DemoDamageEventService();
            DamagePolicyService = new DemoDamagePolicyService();
            UnitOwnerInfoProvider = new DemoUnitOwnerInfoProvider();
            CombatPropertyVolumeProvider = new DemoCombatPropertyVolumeProvider();
            TargetQueryService = new DemoTargetQueryService();
            SubobjectModelOverrideProvider = new DemoSubobjectModelOverrideProvider();
            SkillLogicOverrideProvider = new DemoSkillLogicOverrideProvider();
            BattleEffectService = new DemoBattleEffectService();
            BattleAudioService = new DemoBattleAudioService();
            BattleLogService = new DemoBattleLogService();
            ModeLogicService = new DemoBattleModeLogicService();
            DeathProcessService = new DemoDeathProcessService();
            SubobjectTransferEffectService = new DemoSubobjectTransferEffectService();
        }

        public DemoBattlePlayerInfo FriendPlayer { get; set; } = new DemoBattlePlayerInfo(1, 0, true);

        public DemoBattlePlayerInfo EnemyPlayer { get; set; } = new DemoBattlePlayerInfo(2, 1, false);

        public float DemoMaxDurationSeconds { get; set; } = 60f;

        public bool DemoAutoFinishWhenSideDead { get; set; } = true;
    }
}
