using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Entitas;
using LccModel;
using UnityEngine;

namespace LccHotfix
{
    public class SysInitializeBattle : IInitializeSystem
    {
        private ECSWorld _world;

        public SysInitializeBattle(ECSWorld world)
        {
            _world = world;
        }

        public void Initialize()
        {
            InitEntityIndex();
            InitComponent();
        }

        private void InitEntityIndex()
        {
            _world.LogicContext.AddEntityIndex(new PrimaryEntityIndex<LogicEntity, long>(typeof(ComID).Name, _world.LogicContext.GetGroup(LogicMatcher.AllOf(LogicComponentsLookup.ComID)), (e, c) => ((ComID)c).id));
            _world.LogicContext.AddEntityIndex(new EntityIndexEnum<LogicEntity, TagType>(typeof(ComTag).Name, _world.LogicContext.GetGroup(LogicMatcher.AllOf(LogicComponentsLookup.ComTag)), (e, c) => ((ComTag)c).tag));
            _world.LogicContext.AddEntityIndex(new EntityIndex<LogicEntity, FactionType>(typeof(ComFaction).Name, _world.LogicContext.GetGroup(LogicMatcher.AllOf(LogicComponentsLookup.ComFaction)), (e, c) => ((ComFaction)c).faction));
            _world.LogicContext.AddEntityIndex(new EntityIndex<LogicEntity, long>(typeof(ComOwnerEntity).Name, _world.LogicContext.GetGroup(LogicMatcher.AllOf(LogicComponentsLookup.ComOwnerEntity)), (e, c) => ((ComOwnerEntity)c).ownerEntityID));
            _world.LogicContext.AddEntityIndex(new GroupEntityIndex<LogicEntity, int>(typeof(ComUnityObjectRelated).Name, _world.LogicContext.GetGroup(LogicMatcher.AllOf(LogicComponentsLookup.ComUnityObjectRelated)), (e, c) => ((ComUnityObjectRelated)c).gameObjectInstanceID.Keys.ToArray()));
        }

        private void InitComponent()
        {
            var mode = new BattleMode();
            mode.world = _world;
            _world.MetaContext.SetComUniGameMode(mode);
            _world.MetaContext.SetComUniCameraBlender(new SmoothFoolow2D());
            _world.MetaContext.SetComUniInGamePlayers(_world.GetWorldData<BattleWorldData>().PlayerList);
            _world.MetaContext.SetComUniOrca();
            _world.MetaContext.SetComUniDamage(new DamageBase());
            _world.MetaContext.SetComUniFloatingText(new DNPFloatingText());
            _world.MetaContext.SetComUniJob();
        }
    }
}