using System.Collections.Generic;
using Entitas;

namespace LccHotfix
{
    public sealed class SysMainFSM : IExecuteSystem
    {
        private readonly LogicWorld _logicWorld;
        private readonly MetaWorld _metaWorld;
        private readonly IGroup<LogicEntity> _group;

        private readonly List<LogicEntity> _entityBuffer = new(256);
        private int _entityBufferVersion = -1;

        public SysMainFSM(ECWorlds world)
        {
            _logicWorld = world.LogicWorld;
            _metaWorld = world.MetaWorld;
            _group = _logicWorld.GetGroup(LogicMatcher.AllOf(LogicComponentsLookup.ComMainFSM));
        }

        void IExecuteSystem.Execute()
        {
            var dt = BattleTime.GetDeltaTime(_logicWorld);
            var buffer = _group.GetEntities(_entityBuffer, ref _entityBufferVersion);
            foreach (var e in buffer)
            {
                var entityDt = dt * BattleBulletTimeUtility.GetCompensateRatio(e, _metaWorld);
                e.comFSM.Logic.Update(entityDt);
            }
        }

    }
}
