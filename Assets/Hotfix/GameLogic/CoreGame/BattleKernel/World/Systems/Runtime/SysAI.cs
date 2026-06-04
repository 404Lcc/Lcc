using Entitas;
using UnityEngine;

namespace LccHotfix
{
    public class SysAI : IExecuteSystem
    {
        private readonly IGroup<LogicEntity> _group;
        private readonly LogicWorld _logicWorld;
        
        public SysAI(ECWorlds world)
        {
            _logicWorld = world.LogicWorld;
            _group = _logicWorld.GetGroup(LogicMatcher.AllOf(LogicComponentsLookup.ComAI));
        }
        
        public void Execute()
        {
            var dt = BattleTime.GetDeltaTime(_logicWorld);
            foreach (var e in _group.GetEntities())
            {
                e.comAI.Logic.Update(dt);
            }
        }
    }
}
