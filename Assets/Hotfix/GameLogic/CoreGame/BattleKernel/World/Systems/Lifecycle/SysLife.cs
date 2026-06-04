using Entitas;
using UnityEngine;

namespace LccHotfix
{
    public class SysLife : IExecuteSystem
    {
        private readonly IGroup<LogicEntity> _group;
        private readonly LogicWorld _logicWorld;
        
        public SysLife(ECWorlds worlds)
        {
            _logicWorld = worlds.LogicWorld;
            _group = _logicWorld.GetGroup(LogicMatcher.AllOf(LogicComponentsLookup.ComLife));
        }

        private static int acc = 0;
        public void Execute()
        {
            var dt = BattleTime.GetDeltaTime(_logicWorld);
            foreach (var e in _group.GetEntities())
            {
                var comLife = e.comLife;
                if (comLife.duration > 0)
                {
                    comLife.duration -= dt;
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
