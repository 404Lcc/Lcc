using Entitas;
using UnityEngine;

namespace LccHotfix
{
    public class SysSkillSlot : IExecuteSystem
    {
        private readonly IGroup<LogicEntity> _group;
        private LogicWorld _logicWorld;

        public SysSkillSlot(ECWorlds world)
        {
            _logicWorld = world.LogicWorld;
            _group = _logicWorld.GetGroup(LogicMatcher.AllOf(LogicComponentsLookup.ComSkillSlot));
        }

        void IExecuteSystem.Execute()
        {
            var dt = BattleTime.GetDeltaTime(_logicWorld);
            foreach (var e in _group.GetEntities())
            {
                // tick各技能释放cd
                foreach (var skillSlot in e.comSkillSlot.skillSlots)
                {
                    if (skillSlot.CdTimer > 0)
                    {
                        skillSlot.CdTimer -= dt;
                        if (skillSlot.CdTimer < 0)
                        {
                            skillSlot.CdTimer = 0;
                        }
                    }
                }
            }
        }

    }
}
