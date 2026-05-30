namespace LccHotfix
{
    public partial class LogicCfgContainerRegister
    {
        partial void Init(ICustomLogicService service)
        {
            service.AddConfigContainer(new LogicConfigs_GameMode(LogicContainerKey.LogicConfigs_GameMode));
            service.AddConfigContainer(new LogicConfigs_EntityFSM(LogicContainerKey.LogicConfigs_EntityFSM));
            service.AddConfigContainer(new LogicConfigs_Skill(LogicContainerKey.LogicConfigs_Skill));
            service.AddConfigContainer(new LogicConfigs_Subobject(LogicContainerKey.LogicConfigs_Subobject));
        }
    }
}
