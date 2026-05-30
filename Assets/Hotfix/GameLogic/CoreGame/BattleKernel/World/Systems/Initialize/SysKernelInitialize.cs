using System.Linq;
using Entitas;

namespace LccHotfix
{
    public class SysKernelInitialize : SysBase, IInitializeSystem
    {
        public SysKernelInitialize(ECWorlds world) : base(world)
        {
        }

        public void Initialize()
        {
            InitServices();
            InitEntityIndex();
        }

        private void InitServices()
        {
            var creationInfo = _world.GetCreationInfo<BattleKernelCreationInfo>();
            _world.LogicWorld.SetTimerService(creationInfo.TimerService);
            _world.LogicWorld.SetBattleFeedbackSink(creationInfo.BattleFeedbackSink);
            _world.LogicWorld.SetViewLoadService(creationInfo.ViewLoadService);
            _world.LogicWorld.SetMainObjectViewType(creationInfo.MainObjectViewType);
            _world.LogicWorld.SetGizmoService(creationInfo.GizmoService);
            _world.LogicWorld.SetCustomLogicService(creationInfo.CustomLogicService);
            _world.LogicWorld.SetDamagePropertyModifier(creationInfo.DamagePropertyModifier);
            _world.LogicWorld.SetDamageEventService(creationInfo.DamageEventService);
            _world.LogicWorld.SetDamagePolicyService(creationInfo.DamagePolicyService);
            _world.LogicWorld.SetUnitOwnerInfoProvider(creationInfo.UnitOwnerInfoProvider);
            _world.LogicWorld.SetCombatPropertyVolumeProvider(creationInfo.CombatPropertyVolumeProvider);
            _world.LogicWorld.SetTargetQueryService(creationInfo.TargetQueryService);
            _world.LogicWorld.SetSubobjectModelOverrideProvider(creationInfo.SubobjectModelOverrideProvider);
            _world.LogicWorld.SetSkillLogicOverrideProvider(creationInfo.SkillLogicOverrideProvider);
            _world.LogicWorld.SetBattleEffectService(creationInfo.BattleEffectService);
            _world.LogicWorld.SetBattleAudioService(creationInfo.BattleAudioService);
            _world.LogicWorld.SetBattleLogService(creationInfo.BattleLogService);
            _world.LogicWorld.SetDeathProcessService(creationInfo.DeathProcessService);
            _world.LogicWorld.SetSubobjectTransferEffectService(creationInfo.SubobjectTransferEffectService);
        }

        private void InitEntityIndex()
        {
            var logicWorld = _world.LogicWorld;
            logicWorld.AddEntityIndex(new PrimaryEntityIndex<LogicEntity, long>(EntityIndexName.IDComponent, logicWorld.GetGroup(LogicMatcher.AllOf(LogicComponentsLookup.ComID)), (e, c) => ((IDComponent)c).id));
            logicWorld.AddEntityIndex(new EntityIndex<LogicEntity, EFaction>(EntityIndexName.FactionComponent, logicWorld.GetGroup(LogicMatcher.AllOf(LogicComponentsLookup.ComFaction)), (e, c) => ((FactionComponent)c).Faction));
            logicWorld.AddEntityIndex(new EntityIndex<LogicEntity, long>(EntityIndexName.HolderEntityComponent, logicWorld.GetGroup(LogicMatcher.AllOf(LogicComponentsLookup.ComHolderEntity)), (e, c) => ((HolderEntityComponent)c).HolderEntityID));
            logicWorld.AddEntityIndex(new GroupEntityIndex<LogicEntity, int>(EntityIndexName.UnityObjectRelatedComponent, logicWorld.GetGroup(LogicMatcher.AllOf(LogicComponentsLookup.ComUnityObjectRelated)), (e, c) => ((UnityObjectRelatedComponent)c).gameObjectInstanceID.Keys.ToArray()));
            logicWorld.AddEntityIndex(new EntityIndex<LogicEntity, int>(EntityIndexName.BattleUnitTagComponent, logicWorld.GetGroup(LogicMatcher.AllOf(LogicComponentsLookup.ComBattleUnitTag)), (e, c) => ((BattleUnitTagComponent)c).Tag.BattleUnitTid ?? 0));
        }
    }
}
