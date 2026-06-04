using Entitas;
using UnityEngine;

namespace LccHotfix
{
    public sealed class SysBuff : IExecuteSystem
    {
        private readonly IGroup<LogicEntity> _group;
        private readonly LogicWorld _logicWorld;

        public SysBuff(ECWorlds world)
        {
            _logicWorld = world.LogicWorld;
            _group = _logicWorld.GetGroup(LogicMatcher.AllOf(LogicComponentsLookup.ComBuffCenter));
        }

        void IExecuteSystem.Execute()
        {
            var dt = BattleTime.GetDeltaTime(_logicWorld);
            foreach (var e in _group.GetEntities())
            {
                e.comBuffCenter.Update(dt);
            }
        }

    }
}
