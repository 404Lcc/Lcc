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

        public static int TotalComponents => componentTypes.Length;
        public static List<string> componentNameList;
        public static List<Type> componentTypeList;
        public static string[] componentNames => componentNameList.ToArray();
        public static Type[] componentTypes => componentTypeList.ToArray();
    }
}