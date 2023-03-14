using System;

namespace LccModel
{
    public interface IEvent
    {
        Type Type { get; }
    }
}