using Entitas;

namespace LccHotfix
{
    public class SysSkillProcess : IExecuteSystem, ILateUpdateSystem
    {
        private IGroup<LogicEntity> _group;

        public SysSkillProcess(ECSWorld world)
        {
            _group = world.LogicContext.GetGroup(LogicMatcher.AllOf(LogicMatcher.ComSkillProcess));
        }

        public void LateUpdate()
        {
            foreach (var item in _group.GetEntities())
            {
                var comSkillProcess = item.comSkillProcess;
                var process = comSkillProcess.skillProcess;
                if (process != null)
                {
                    process.LateUpdate();
                }
            }
        }

        void IExecuteSystem.Execute()
        {
            foreach (var item in _group.GetEntities())
            {
                var comSkillProcess = item.comSkillProcess;
                var process = comSkillProcess.skillProcess;
                if (process == null)
                {
                    item.RemoveComSkillProcess();
                }
                else
                {
                    process.Update();

                    if (process.IsFinished())
                    {
                        item.RemoveComSkillProcess();
                    }
                }
            }
        }
    }
}