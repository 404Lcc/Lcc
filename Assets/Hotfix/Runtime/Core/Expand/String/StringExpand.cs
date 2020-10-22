using System;
using UnityEngine;

namespace LccHotfix
{
    public static class StringExpand
    {
        public static PanelType ToPanelType(this string name)
        {
            name = name.Substring(0, name.IndexOf("Panel"));
            return (PanelType)Enum.Parse(typeof(PanelType), name);
        }
        public static GameObject GetGameObjectToName(this string name)
        {
            return GameObject.Find(name);
        }
        public static GameObject GetGameObjectToTag(this string tag)
        {
            return GameObject.FindGameObjectWithTag(tag);
        }
        public static GameObject[] GetGameObjectsToTag(this string tag)
        {
            return GameObject.FindGameObjectsWithTag(tag);
        }
    }
}