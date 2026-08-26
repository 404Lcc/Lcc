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
            BattleLogger.SetService(creationInfo.BattleLogService);
        }

        private void InitEntityIndex()
        {
            var logicWorld = _world.LogicWorld;
            logicWorld.AddEntityIndex(new PrimaryEntityIndex<LogicEntity, long>(EntityIndexName.IDComponent, logicWorld.GetGroup(LogicMatcher.AllOf(LogicComponentsLookup.ComID)), (e, c) => ((IDComponent)c).ID));
            logicWorld.AddEntityIndex(new EntityIndex<LogicEntity, EFaction>(EntityIndexName.FactionComponent, logicWorld.GetGroup(LogicMatcher.AllOf(LogicComponentsLookup.ComFaction)), (e, c) => ((FactionComponent)c).Faction));
            // 可战斗单位按阵营索引：供 TargetUtility 索敌，避免扫全体
            var fighterGroup = logicWorld.GetGroup(
                LogicMatcher.AllOf(
                    LogicComponentsLookup.ComFaction,
                    LogicComponentsLookup.ComView
                ).AnyOf(
                    LogicComponentsLookup.ComMonster,
                    LogicComponentsLookup.ComHero
                ).NoneOf(
                    LogicComponentsLookup.ComDeath)
                );
            // getKey 必须容错：多组件 Group 进出时 c 可能是 View/Death 等非 Faction 组件
            logicWorld.AddEntityIndex(new EntityIndex<LogicEntity, EFaction>(
                EntityIndexName.FactionFighter,
                fighterGroup,
                (e, c) => c is FactionComponent fc ? fc.Faction : e.comFaction.Faction));
            logicWorld.AddEntityIndex(new EntityIndex<LogicEntity, long>(EntityIndexName.HolderEntityComponent, logicWorld.GetGroup(LogicMatcher.AllOf(LogicComponentsLookup.ComHolderEntity)), (e, c) => ((HolderEntityComponent)c).HolderEntityID));
            logicWorld.AddEntityIndex(new GroupEntityIndex<LogicEntity, int>(EntityIndexName.UnityObjectRelatedComponent, logicWorld.GetGroup(LogicMatcher.AllOf(LogicComponentsLookup.ComUnityObjectRelated)), (e, c) => ((UnityObjectRelatedComponent)c).gameObjectInstanceID.Keys.ToArray()));
            logicWorld.AddEntityIndex(new EntityIndex<LogicEntity, int>(EntityIndexName.BattleUnitTagComponent, logicWorld.GetGroup(LogicMatcher.AllOf(LogicComponentsLookup.ComBattleUnitTag)), (e, c) => ((BattleUnitTagComponent)c).Tag.BattleUnitTid ?? 0));
            logicWorld.AddEntityIndex_ComTag();
            logicWorld.AddEntityIndex_Name();
        }
    }
}
