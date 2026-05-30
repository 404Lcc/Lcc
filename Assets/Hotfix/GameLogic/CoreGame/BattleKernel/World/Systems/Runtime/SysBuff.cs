using Entitas;
using UnityEngine;

namespace LccHotfix
{
    public sealed class SysBuff : IExecuteSystem
    {
        private readonly IGroup<LogicEntity> _group;

        public SysBuff(ECWorlds world)
        {
            _group = world.LogicWorld.GetGroup(LogicMatcher.AllOf(LogicComponentsLookup.ComBuffCenter));
        }

        void IExecuteSystem.Execute()
        {
            var dt = Time.deltaTime;
            foreach (var e in _group.GetEntities())
            {
                e.comBuffCenter.Update(dt);
            }
        }

    }
}
