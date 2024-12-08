using System;
using System.Collections.Generic;

namespace LccHotfix
{
    public static partial class LogicComponentsLookup
    {
        public static int ComID;
        public static int ComTag;
        public static int ComFaction;
        public static int ComOwnerEntity;
        public static int ComUnityObjectRelated;

        public static int TotalComponents => componentTypes.Count;

        public static List<string> componentNames;

        public static List<Type> componentTypes;
    }
}