using System;

namespace LccHotfix
{
    public interface IViewLoadService
    {
        void LoadObjectAsync(string objName, Action<IReceiveLoaded> onComplete);
    }
}
