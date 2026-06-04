using HotUpdate.Framework;

namespace LccHotfix
{
    public class LogicCfgContainerRegister_Demo : ILogicCfgContainerRegister
    {
        public void Register(ICustomLogicService service)
        {
            service.AddConfigContainer(new LogicConfigs_DemoGameMode(LogicContainerKey.LogicConfigs_GameMode));
            service.AddConfigContainer(new LogicConfig_DemoEntityFSM(LogicContainerKey.LogicConfigs_EntityFSM));
            service.AddConfigContainer(new LogicConfigs_DemoSkill(LogicContainerKey.LogicConfigs_Skill));
            service.AddConfigContainer(new LogicConfigs_DemoSubobject(LogicContainerKey.LogicConfigs_Subobject));
        }
    }
}
