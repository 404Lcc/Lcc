using Entitas;
using UnityEngine;

namespace LccHotfix
{
    public class SysLife : IExecuteSystem
    {
        private readonly IGroup<LogicEntity> _group;
        
        public SysLife(ECWorlds worlds)
        {
            _group = worlds.LogicWorld.GetGroup(LogicMatcher.AllOf(LogicComponentsLookup.ComLife));
        }

        private static int acc = 0;
        public void Execute()
        {
            var dt = Time.deltaTime;
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
