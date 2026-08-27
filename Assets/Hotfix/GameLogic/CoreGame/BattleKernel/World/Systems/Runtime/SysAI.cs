using System.Collections.Generic;
using Entitas;
using UnityEngine;

namespace LccHotfix
{
    public class SysAI : IExecuteSystem
    {
        private readonly IGroup<LogicEntity> _group;
        private readonly LogicWorld _logicWorld;
        private readonly MetaWorld _metaWorld;

        private readonly List<LogicEntity> _entityBuffer = new(256);

        public SysAI(ECWorlds world)
        {
            _logicWorld = world.LogicWorld;
            _metaWorld = world.MetaWorld;
            _group = _logicWorld.GetGroup(LogicMatcher.AllOf(LogicComponentsLookup.ComAI));
        }

        public void Execute()
        {
            var dt = BattleTime.GetDeltaTime(_logicWorld);
            var buffer = _group.GetEntities(_entityBuffer);
            foreach (var e in buffer)
            {
                var entityDt = dt * BattleBulletTimeUtility.GetCompensateRatio(e, _metaWorld);
                e.comAI.Logic.Update(entityDt);
            }
        }
    }
}
