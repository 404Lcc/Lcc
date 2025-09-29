using System;
using System.Collections.Generic;

namespace LccHotfix
{
    public static partial class MetaComponentsLookup
    {
        public static List<ComponentTypeIndex> typeIndexList = new List<ComponentTypeIndex>();
        public static int TotalComponents => componentTypes.Length;
        public static List<string> componentNameList;
        public static List<Type> componentTypeList;
        public static string[] componentNames => componentNameList.ToArray();
        public static Type[] componentTypes => componentTypeList.ToArray();
    }
}