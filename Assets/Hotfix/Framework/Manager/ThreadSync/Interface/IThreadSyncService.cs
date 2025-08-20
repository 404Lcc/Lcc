using System;

namespace LccHotfix
{
    public interface IThreadSyncService : IService
    {
        void Post(Action action);
    }
}