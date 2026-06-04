using Entitas;
using UnityEngine;

namespace LccHotfix
{
    public sealed class SysMainFSM : IExecuteSystem
    {
        private readonly ECWorlds _world;
        private readonly IGroup<LogicEntity> _group;

        public SysMainFSM(ECWorlds world)
        {
            _world = world;
            _group = _world.LogicWorld.GetGroup(LogicMatcher.AllOf(LogicComponentsLookup.ComMainFSM));
        }

        void IExecuteSystem.Execute()
        {
            var dt = BattleTime.GetDeltaTime(_world.LogicWorld);
            foreach (var e in _group.GetEntities())
            {
                e.comFSM.Logic.Update(dt);
            }
        }

    }
}
