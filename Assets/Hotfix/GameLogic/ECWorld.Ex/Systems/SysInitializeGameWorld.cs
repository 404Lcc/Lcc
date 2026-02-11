using System.Linq;
using Entitas;

namespace LccHotfix
{
    public class SysInitializeGameWorld : SysBase, IInitializeSystem, ITearDownSystem
    {
        private ECGameWorld _world;

        public SysInitializeGameWorld(ECGameWorld world) : base(world)
        {
            _world = world;
        }

        public void Initialize()
        {
            InitEntityIndex();
            AddMetaComponents();
        }

        public void InitEntityIndex()
        {
            var logicWorld = _world.LogicWorld;
            logicWorld.AddEntityIndex(new PrimaryEntityIndex<LogicEntity, long>("IDComponent", logicWorld.GetGroup(LogicMatcher.AllOf(LogicComponentsLookup.ComID)), (e, c) => ((IDComponent)c).id));
            logicWorld.AddEntityIndex(new EntityIndex<LogicEntity, EFaction>("FactionComponent", logicWorld.GetGroup(LogicMatcher.AllOf(LogicComponentsLookup.ComFaction)), (e, c) => ((FactionComponent)c).Faction));
            logicWorld.AddEntityIndex(new EntityIndex<LogicEntity, long>("OwnerEntityComponent", logicWorld.GetGroup(LogicMatcher.AllOf(LogicComponentsLookup.ComOwnerEntity)), (e, c) => ((OwnerEntityComponent)c).OwnerEntityID));
            logicWorld.AddEntityIndex(new EntityIndex<LogicEntity, uint>("TagComponent", logicWorld.GetGroup(LogicMatcher.AllOf(LogicComponentsLookup.ComTag)), (e, c) => ((TagComponent)c).Tags));
            logicWorld.AddEntityIndex(new GroupEntityIndex<LogicEntity, int>("UnityObjectRelatedComponent", logicWorld.GetGroup(LogicMatcher.AllOf(LogicComponentsLookup.ComUnityObjectRelated)), (e, c) => ((UnityObjectRelatedComponent)c).gameObjectInstanceID.Keys.ToArray()));
        }

        private void AddMetaComponents()
        {
            var metaWorld = _world.MetaWorld;
            var logicWorld = _world.LogicWorld;

            metaWorld.SetComUniMap("Map");
        }

        public void TearDown()
        {
            Main.GameObjectPoolService.CancelAll();
        }
    }
}