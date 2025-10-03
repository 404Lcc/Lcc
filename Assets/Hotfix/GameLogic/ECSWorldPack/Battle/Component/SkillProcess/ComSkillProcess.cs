namespace LccHotfix
{
    //技能执行体
    public class ComSkillProcess : LogicComponent
    {
        public ISkillProcess skillProcess;
    }


    public partial class LogicEntity
    {

        public ComSkillProcess comSkillProcess
        {
            get { return (ComSkillProcess)GetComponent(LogicComponentsLookup.ComSkillProcess); }
        }

        public bool hasComSkillProcess
        {
            get { return HasComponent(LogicComponentsLookup.ComSkillProcess); }
        }

        public void AddComSkillProcess(ISkillProcess newSkillProcess)
        {
            var index = LogicComponentsLookup.ComSkillProcess;
            var component = (ComSkillProcess)CreateComponent(index, typeof(ComSkillProcess));
            component.skillProcess = newSkillProcess;
            AddComponent(index, component);
        }

        public void ReplaceComSkillProcess(ISkillProcess newSkillProcess)
        {
            var index = LogicComponentsLookup.ComSkillProcess;
            var component = (ComSkillProcess)CreateComponent(index, typeof(ComSkillProcess));
            component.skillProcess = newSkillProcess;
            ReplaceComponent(index, component);
        }

        public void RemoveComSkillProcess()
        {
            comSkillProcess.skillProcess.Dispose();
            RemoveComponent(LogicComponentsLookup.ComSkillProcess);
        }
    }

    public static partial class LogicComponentsLookup
    {
        private static ComponentTypeIndex ComSkillProcessIndex = new ComponentTypeIndex(typeof(ComSkillProcess));
        public static int ComSkillProcess => ComSkillProcessIndex.index;
    }
}