namespace LccHotfix
{

    public static partial class CLogger
    {
#if CONSOLE_CLIENT
        public static void AssertBreak()
        {
            CLogger.LogError("AssertBreak: Logic Has Core ERROR! ");
        }

        public static void Log(string info)
        {
        }

        public static void LogInfo(string info)
        {
        }

        public static void LogError(string info)
        {
        }

        public static void LogDebug(string info)
        {
        }

#else
        public static void AssertBreak()
        {
            UnityEngine.Debug.LogError("AssertBreak: Logic Has Core ERROR! ");
            UnityEngine.Debug.Break();

        }

        public static void Log(string info)
        {
            UnityEngine.Debug.Log(info);
        }

        public static void LogInfo(string info)
        {
            UnityEngine.Debug.Log(info);
        }

        public static void LogError(string info)
        {
            UnityEngine.Debug.LogError(info);
        }

        public static void LogDebug(string info)
        {
            string line = string.Format("{0}{1}{2}", "<color=#22BB00FF>", info, "</color>");
            UnityEngine.Debug.LogError(line);
        }
#endif
    }
}