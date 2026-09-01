using System.Collections.Generic;
using Entitas;

namespace LccHotfix
{
    public class SysSkillSlot : IExecuteSystem
    {
        private readonly IGroup<LogicEntity> _group;
        private readonly LogicWorld _logicWorld;
        private readonly MetaWorld _metaWorld;

        private readonly List<LogicEntity> _entityBuffer = new(256);
        private int _entityBufferVersion = -1;

        public SysSkillSlot(ECWorlds world)
        {
            _logicWorld = world.LogicWorld;
            _metaWorld = world.MetaWorld;
            _group = _logicWorld.GetGroup(LogicMatcher.AllOf(LogicComponentsLookup.ComSkillSlot));
        }

        void IExecuteSystem.Execute()
        {
            var dt = BattleTime.GetDeltaTime(_logicWorld);
            var buffer = _group.GetEntities(_entityBuffer, ref _entityBufferVersion);
            foreach (var e in buffer)
            {
                var entityDt = dt * BattleBulletTimeUtility.GetCompensateRatio(e, _metaWorld);
                // tick各技能释放cd
                foreach (var skillSlot in e.comSkillSlot.skillSlots)
                {
                    if (skillSlot.CdTimer > 0)
                    {
                        skillSlot.CdTimer -= entityDt;
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
