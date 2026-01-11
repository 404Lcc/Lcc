using System;
using System.Collections.Generic;

namespace LccHotfix
{
    public class ComponentTypeIndex
    {
        public Type CmptType;
        public int Index;

        public ComponentTypeIndex(Type type, int index = -1)
        {
            CmptType = type;
            Index = -1;
        }
    }

    public static partial class LogicComponentsLookup
    {
        public static List<ComponentTypeIndex> TypeIndexList = new List<ComponentTypeIndex>();
        public static int TotalComponents => TypeIndexList.Count;
    }
}