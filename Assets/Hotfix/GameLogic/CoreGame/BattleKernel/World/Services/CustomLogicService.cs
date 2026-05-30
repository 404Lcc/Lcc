using HotUpdate.Framework;

namespace LccHotfix
{
    public partial class LogicWorld
    {
        public ICustomLogicService CustomLogicService { get; private set; }

        public void SetCustomLogicService(ICustomLogicService customLogicService)
        {
            CustomLogicService = customLogicService;
        }
    }
}
