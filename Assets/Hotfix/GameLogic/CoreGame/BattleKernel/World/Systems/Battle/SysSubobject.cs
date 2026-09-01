using System.Collections.Generic;
using Entitas;

namespace LccHotfix
{
    public class SysSubobject : IExecuteSystem
    {
        private readonly IGroup<LogicEntity> _group;
        private readonly LogicWorld _logicWorld;
        private readonly MetaWorld _metaWorld;

        private readonly List<LogicEntity> _entityBuffer = new(256);
        private int _entityBufferVersion = -1;

        public SysSubobject(ECWorlds world)
        {
            _logicWorld = world.LogicWorld;
            _metaWorld = world.MetaWorld;
            _group = _logicWorld.GetGroup(LogicMatcher.AllOf(LogicComponentsLookup.ComSubobject));
        }

        public void Execute()
        {
            var dt = BattleTime.GetDeltaTime(_logicWorld);
            var buffer = _group.GetEntities(_entityBuffer, ref _entityBufferVersion);
            foreach (var entity in buffer)
            {
                var comSubobject = entity.comSubobject;
                if (comSubobject.Logic != null)
                {
                    var entityDt = dt * BattleBulletTimeUtility.GetCompensateRatio(entity, _metaWorld);
                    comSubobject.Logic.Update(entityDt);
                }
            }
        }
    }
}
