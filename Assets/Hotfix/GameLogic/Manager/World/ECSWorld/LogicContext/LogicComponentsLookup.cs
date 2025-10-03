using System;
using System.Collections.Generic;

namespace LccHotfix
{
    public static partial class LogicComponentsLookup
    {
        public static List<ComponentTypeIndex> typeIndexList = new List<ComponentTypeIndex>();
        public static int TotalComponents => typeIndexList.Count;
    }
}