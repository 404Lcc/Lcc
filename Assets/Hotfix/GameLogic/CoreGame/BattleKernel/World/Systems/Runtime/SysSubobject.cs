using Entitas;
using UnityEngine;

namespace LccHotfix
{
    public class SysSubobject : IExecuteSystem
    {
        private readonly IGroup<LogicEntity> _group;
        private readonly LogicWorld _logicWorld;

        public SysSubobject(ECWorlds world)
        {
            _logicWorld = world.LogicWorld;
            _group = _logicWorld.GetGroup(LogicMatcher.AllOf(LogicComponentsLookup.ComSubobject));
        }

        public void Execute()
        {
            var dt = BattleTime.GetDeltaTime(_logicWorld);
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
