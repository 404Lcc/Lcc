using System;

namespace LccHotfix
{
    public interface IViewLoadService
    {
        void LoadObjectAsync(string objName, Action<IReceiveLoaded> onComplete);
    }

    public partial class LogicWorld
    {
        public IViewLoadService ViewLoadService { get; private set; }

        public void SetViewLoadService(IViewLoadService viewLoadService)
        {
            ViewLoadService = viewLoadService;
        }
    }
}
