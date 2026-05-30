using System;
using System.Runtime.CompilerServices;

namespace LccHotfix
{
    public interface IBattleLogService
    {
        bool IsDebugEnabled { get; }
        void Debug(string info, string path = null, string memberName = null, int lineNumber = 0);
        void Warning(string info, string path = null, string memberName = null, int lineNumber = 0);
        void Error(string info, string path = null, string memberName = null, int lineNumber = 0);
        void Exception(Exception exception, string path = null, string memberName = null, int lineNumber = 0);
    }

    public static class BattleLog
    {
        private static IBattleLogService s_Service;

        public static bool IsDebugEnabled => s_Service?.IsDebugEnabled == true;

        public static void SetService(IBattleLogService service)
        {
            s_Service = service;
        }

        public static void Debug(string info, [CallerFilePath] string path = null, [CallerMemberName] string memberName = null, [CallerLineNumber] int lineNumber = 0)
        {
            s_Service?.Debug(info, path, memberName, lineNumber);
        }

        public static void Warning(string info, [CallerFilePath] string path = null, [CallerMemberName] string memberName = null, [CallerLineNumber] int lineNumber = 0)
        {
            s_Service?.Warning(info, path, memberName, lineNumber);
        }

        public static void Error(string info, [CallerFilePath] string path = null, [CallerMemberName] string memberName = null, [CallerLineNumber] int lineNumber = 0)
        {
            s_Service?.Error(info, path, memberName, lineNumber);
        }

        public static void Exception(Exception exception, [CallerFilePath] string path = null, [CallerMemberName] string memberName = null, [CallerLineNumber] int lineNumber = 0)
        {
            s_Service?.Exception(exception, path, memberName, lineNumber);
        }
    }

    public partial class LogicWorld
    {
        public IBattleLogService BattleLogService { get; private set; }

        public void SetBattleLogService(IBattleLogService service)
        {
            BattleLogService = service;
            BattleLog.SetService(service);
        }
    }
}
