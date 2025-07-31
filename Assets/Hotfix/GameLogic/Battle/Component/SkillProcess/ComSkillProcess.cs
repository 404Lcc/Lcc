namespace LccHotfix
{
    //技能执行体
    public class ComSkillProcess : LogicComponent
    {
        public ISkillProcess skillProcess;
    }


    public partial class LogicEntity
    {

        public ComSkillProcess comSkillProcess { get { return (ComSkillProcess)GetComponent(LogicComponentsLookup.ComSkillProcess); } }
        public bool hasComSkillProcess { get { return HasComponent(LogicComponentsLookup.ComSkillProcess); } }

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
    public sealed partial class LogicMatcher
    {

        private static Entitas.IMatcher<LogicEntity> _matcherComSkillProcess;

        public static Entitas.IMatcher<LogicEntity> ComSkillProcess
        {
            get
            {
                if (_matcherComSkillProcess == null)
                {
                    var matcher = (Entitas.Matcher<LogicEntity>)Entitas.Matcher<LogicEntity>.AllOf(LogicComponentsLookup.ComSkillProcess);
                    matcher.ComponentNames = LogicComponentsLookup.componentNames;
                    _matcherComSkillProcess = matcher;
                }

                return _matcherComSkillProcess;
            }
        }
    }
    public static partial class LogicComponentsLookup
    {
        public static int ComSkillProcess;
    }
}