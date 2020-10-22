using System;

namespace LccHotfix
{
    public interface IEvent
    {
        Type EventType();
    }
}