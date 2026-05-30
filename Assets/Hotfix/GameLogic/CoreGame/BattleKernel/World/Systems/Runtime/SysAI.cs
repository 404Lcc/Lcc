using Entitas;
using UnityEngine;

namespace LccHotfix
{
    public class SysAI : IExecuteSystem
    {
        private readonly IGroup<LogicEntity> _group;
        
        public SysAI(ECWorlds world)
        {
            _group = world.LogicWorld.GetGroup(LogicMatcher.AllOf(LogicComponentsLookup.ComAI));
        }
        
        public void Execute()
        {
            var dt = Time.deltaTime;
            foreach (var e in _group.GetEntities())
            {
                e.comAI.Logic.Update(dt);
            }
        }
    }
}
