using Entitas;

namespace LccHotfix
{
    public class SysSkillProcess : IExecuteSystem, ILateUpdateSystem
    {
        private IGroup<LogicEntity> _group;

        public SysSkillProcess(ECSWorld world)
        {
            _group = world.LogicContext.GetGroup(LogicMatcher.AllOf(LogicComponentsLookup.ComSkillProcess));
        }

        public void LateUpdate()
        {
            foreach (var entity in _group.GetEntities())
            {
                var comSkillProcess = entity.comSkillProcess;
                var process = comSkillProcess.skillProcess;
                if (process != null)
                {
                    process.LateUpdate();
                }
            }
        }

        void IExecuteSystem.Execute()
        {
            foreach (var entity in _group.GetEntities())
            {
                var comSkillProcess = entity.comSkillProcess;
                var process = comSkillProcess.skillProcess;
                if (process == null)
                {
                    entity.RemoveComSkillProcess();
                }
                else
                {
                    process.Update();

                    if (process.IsFinished())
                    {
                        entity.RemoveComSkillProcess();
                    }
                }
            }
        }
    }
}