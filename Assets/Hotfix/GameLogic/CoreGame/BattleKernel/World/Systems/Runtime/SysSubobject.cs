using Entitas;
using UnityEngine;

namespace LccHotfix
{
    public class SysSubobject : IExecuteSystem
    {
        private readonly IGroup<LogicEntity> _group;

        public SysSubobject(ECWorlds world)
        {
            _group = world.LogicWorld.GetGroup(LogicMatcher.AllOf(LogicComponentsLookup.ComSubobject));
        }

        public void Execute()
        {
            var dt = Time.deltaTime;
            foreach (var entity in _group.GetEntities())
            {
                var comSubobject = entity.comSubobject;
                if (comSubobject.Logic != null)
                {
                    comSubobject.Logic.Update(dt);
                }
            }
        }
    }
}