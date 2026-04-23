using System;
using System.Collections.Generic;

namespace LccHotfix
{
    public class ComponentTypeIndex
    {
        public Type ComponentType { get; private set; }
        public int Index { get; set; } = -1;

        public ComponentTypeIndex(Type type)
        {
            ComponentType = type;
        }
    }

    public static partial class LogicComponentsLookup
    {
        public static List<ComponentTypeIndex> TypeIndexList = new List<ComponentTypeIndex>();
        public static int TotalComponents => TypeIndexList.Count;
    }
}