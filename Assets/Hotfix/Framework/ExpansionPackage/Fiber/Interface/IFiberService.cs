using System;

namespace LccHotfix
{
    public interface IFiberService : IService
    {
        bool IsDisposed();
        int Create(SchedulerType schedulerType, int fiberId, Action<Fiber> action);
        int Create(SchedulerType schedulerType, Action<Fiber> action);
        void Remove(int id);
        int Count();
    }
}