using System;

namespace LccHotfix
{
    public interface IValueEventService : IService
    {
        void AddHandle<T>(Action<T> handle) where T : struct, IValueEvent;
        void RemoveHandle<T>(Action<T> handle) where T : struct, IValueEvent;
        void Dispatch<T>(T value) where T : struct, IValueEvent;
    }
}