using System;

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
}
