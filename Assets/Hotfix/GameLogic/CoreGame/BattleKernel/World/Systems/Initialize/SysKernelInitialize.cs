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
            logicWorld.AddEntityIndex(new PrimaryEntityIndex<LogicEntity, long>(EntityIndexName.IDComponent, logicWorld.GetGroup(LogicMatcher.AllOf(LogicComponentsLookup.ComID)), (e, c) => ((IDComponent)c).id));
            logicWorld.AddEntityIndex(new EntityIndex<LogicEntity, EFaction>(EntityIndexName.FactionComponent, logicWorld.GetGroup(LogicMatcher.AllOf(LogicComponentsLookup.ComFaction)), (e, c) => ((FactionComponent)c).Faction));
            logicWorld.AddEntityIndex(new EntityIndex<LogicEntity, long>(EntityIndexName.HolderEntityComponent, logicWorld.GetGroup(LogicMatcher.AllOf(LogicComponentsLookup.ComHolderEntity)), (e, c) => ((HolderEntityComponent)c).HolderEntityID));
            logicWorld.AddEntityIndex(new GroupEntityIndex<LogicEntity, int>(EntityIndexName.UnityObjectRelatedComponent, logicWorld.GetGroup(LogicMatcher.AllOf(LogicComponentsLookup.ComUnityObjectRelated)), (e, c) => ((UnityObjectRelatedComponent)c).gameObjectInstanceID.Keys.ToArray()));
            logicWorld.AddEntityIndex(new EntityIndex<LogicEntity, int>(EntityIndexName.BattleUnitTagComponent, logicWorld.GetGroup(LogicMatcher.AllOf(LogicComponentsLookup.ComBattleUnitTag)), (e, c) => ((BattleUnitTagComponent)c).Tag.BattleUnitTid ?? 0));
        }
    }
}
