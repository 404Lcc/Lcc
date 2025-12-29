using System.Collections.Generic;

namespace LccHotfix
{
    public static partial class MetaComponentsLookup
    {
        public static List<ComponentTypeIndex> TypeIndexList = new List<ComponentTypeIndex>();
        public static int TotalComponents => TypeIndexList.Count;
    }
}