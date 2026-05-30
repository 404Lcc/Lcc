using HotUpdate.Framework;

namespace LccHotfix
{
    public partial class LogicCfgContainerRegister : ILogicCfgContainerRegister
    {
        public void Register(ICustomLogicService service)
        {
            Init(service);
        }

        partial void Init(ICustomLogicService service);
    }
}
