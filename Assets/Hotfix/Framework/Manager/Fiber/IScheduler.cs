using System;

namespace LccHotfix
{
    public interface IScheduler : IDisposable
    {
        void Add(int fiberId);
    }
}