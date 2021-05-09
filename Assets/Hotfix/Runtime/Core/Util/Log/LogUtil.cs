using UnityEngine;

namespace LccHotfix
{
    public static class LogUtil
    {
        public static void Log(string message)
        {
            Debug.Log(message);
        }
        public static void LogError(string message)
        {
            Debug.LogError(message);
        }
    }
}