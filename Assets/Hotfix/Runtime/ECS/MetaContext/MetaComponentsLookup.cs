using System;
using System.Collections.Generic;

namespace LccHotfix
{
    public static partial class MetaComponentsLookup
    {
        public static int TotalComponents => componentTypes.Count;

        public static List<string> componentNames;

        public static List<Type> componentTypes;
    }
}