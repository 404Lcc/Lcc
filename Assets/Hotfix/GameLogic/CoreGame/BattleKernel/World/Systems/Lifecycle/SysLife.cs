using System.Collections.Generic;
using Entitas;

namespace LccHotfix
{
    public class SysLife : IExecuteSystem
    {
        private readonly IGroup<LogicEntity> _group;
        private readonly LogicWorld _logicWorld;
        private readonly MetaWorld _metaWorld;

        private readonly List<LogicEntity> _entityBuffer = new(256);

        public SysLife(ECWorlds worlds)
        {
            _logicWorld = worlds.LogicWorld;
            _metaWorld = worlds.MetaWorld;
            _group = _logicWorld.GetGroup(LogicMatcher.AllOf(LogicComponentsLookup.ComLife));
        }

        public void Execute()
        {
            var dt = BattleTime.GetDeltaTime(_logicWorld);
            var buffer = _group.GetEntities(_entityBuffer);
            foreach (var e in buffer)
            {
                var comLife = e.comLife;
                if (comLife.duration > 0)
                {
                    var entityDt = dt * BattleBulletTimeUtility.GetCompensateRatio(e, _metaWorld);
                    comLife.duration -= entityDt;
                }
                else
                {
                    e.RemoveComLife();
                    e.AddComDeath(null);
                }
            }
        }
    }
}
