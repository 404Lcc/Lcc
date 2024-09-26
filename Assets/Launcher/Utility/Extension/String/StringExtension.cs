using System;
using System.Text;
using UnityEngine;

namespace LccModel
{
    public static class StringExtension
    {
        public static byte[] GetBytes(this string s)
        {
            return Encoding.UTF8.GetBytes(s);
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