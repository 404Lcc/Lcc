using System;
using System.Collections.Generic;
using LccModel;

namespace LccHotfix
{
    public class BadgeConfig
    {
        public Delegate Resolve { get; set; }
        public Type[] Listeners { get; set; }
        public Dictionary<Type, Func<IEventMessage, bool>> ListenerCheckDict { get; set; }
    }
}