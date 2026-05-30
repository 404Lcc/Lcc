using Entitas;
using UnityEngine;

namespace LccHotfix
{
    public class SysSkillProcess : IExecuteSystem
    {
        private readonly IGroup<LogicEntity> _group;
        private LogicWorld _logicWorld;

        public SysSkillProcess(ECWorlds world)
        {
            _logicWorld = world.LogicWorld;
            _group = _logicWorld.GetGroup(LogicMatcher.AllOf(LogicComponentsLookup.ComSkillProcess));
        }


        void IExecuteSystem.Execute()
        {
            var dt = Time.deltaTime;
            foreach (var e in _group.GetEntities())
            {
                var process = e.comSkillProcess.SkillProcess;
                if (process == null)
                {
                    BattleLog.Error("SkillProcessSystem SkillProcess == null");
                    e.RemoveComSkillProcess();
                }
                else
                {
                    process.Update(dt);
                    if (process.CanStop())
                    {
                        if (e.hasComSkillSlot)
                        {
                            e.comSkillSlot.ResetCDBySkillTid(e.comSkillProcess.SkillTid);
                        }
                        e.RemoveComSkillProcess();
                    }
                }
            }
        }
    }
}
