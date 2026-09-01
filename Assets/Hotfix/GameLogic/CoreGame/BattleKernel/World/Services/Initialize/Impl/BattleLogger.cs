using System;
using System.Runtime.CompilerServices;

namespace LccHotfix
{
    public static class BattleLogger
    {
        private static IBattleLogService _service;

        public static bool IsDebugEnabled => _service?.IsDebugEnabled == true;

        public static void SetService(IBattleLogService service)
        {
            _service = service;
        }

        public static void LogDebug(string info, [CallerFilePath] string path = null, [CallerMemberName] string memberName = null, [CallerLineNumber] int lineNumber = 0)
        {
            _service?.Debug(info, path, memberName, lineNumber);
        }

        public static void LogWarning(string info, [CallerFilePath] string path = null, [CallerMemberName] string memberName = null, [CallerLineNumber] int lineNumber = 0)
        {
            _service?.Warning(info, path, memberName, lineNumber);
        }

        public static void LogError(string info, [CallerFilePath] string path = null, [CallerMemberName] string memberName = null, [CallerLineNumber] int lineNumber = 0)
        {
            _service?.Error(info, path, memberName, lineNumber);
        }

        public static void LogException(Exception exception, [CallerFilePath] string path = null, [CallerMemberName] string memberName = null, [CallerLineNumber] int lineNumber = 0)
        {
            _service?.Exception(exception, path, memberName, lineNumber);
        }
    }
}
