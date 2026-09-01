using System.Collections.Generic;
using Entitas;

namespace LccHotfix
{
    public class SysSkillProcess : IExecuteSystem
    {
        private readonly IGroup<LogicEntity> _group;
        private readonly LogicWorld _logicWorld;
        private readonly MetaWorld _metaWorld;

        private readonly List<LogicEntity> _entityBuffer = new(256);
        private int _entityBufferVersion = -1;

        public SysSkillProcess(ECWorlds world)
        {
            _logicWorld = world.LogicWorld;
            _metaWorld = world.MetaWorld;
            _group = _logicWorld.GetGroup(LogicMatcher.AllOf(LogicComponentsLookup.ComSkillProcess));
        }


        void IExecuteSystem.Execute()
        {
            var dt = BattleTime.GetDeltaTime(_logicWorld);
            var buffer = _group.GetEntities(_entityBuffer, ref _entityBufferVersion);
            foreach (var e in buffer)
            {
                if (!e.hasComSkillProcess) 
                    continue;
                var process = e.comSkillProcess.SkillProcess;
                if (process == null)
                {
                    BattleLogger.LogError("SkillProcessSystem SkillProcess == null");
                    e.RemoveComSkillProcess();
                }
                else
                {
                    var entityDt = dt * BattleBulletTimeUtility.GetCompensateRatio(e, _metaWorld);
                    process.Update(entityDt);
                    if (!e.hasComSkillProcess)
                        continue;
                    if (process.CanStop())
                    {
                        if (e.hasComSkillSlot)
                        {
                            e.comSkillSlot.ResetCDBySkillTid(e.comSkillProcess.SkillTid);
                            e.comSkillSlot.ResetCDAfterLast(e.comSkillProcess.SkillTid);
                        }
                        e.RemoveComSkillProcess();
                    }
                }
            }
        }
    }
}
