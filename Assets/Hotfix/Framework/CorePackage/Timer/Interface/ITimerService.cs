using System;

namespace LccHotfix
{
    public interface ITimerService : IService
    {
        int AddTimer(Action<float, int> callback, float intervalSec = 1f, int repeatCount = 1, bool useTimeScale = false);
        bool RemoveTimer(int timerId);
    }
}