using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Text;

namespace LccHotfix
{
    public enum KLogLevel
    {
        Log,
        Warning,
        Error,
        Exception
    }

    public class KLogLevelHandle
    {
        public string path = string.Empty;
        public KLogLevel logLevel = KLogLevel.Log;
    }

    public class KLogCategory
    {
        public string categoryName = string.Empty;
        public string logLevelPath = string.Empty;
        public KLogLevelHandle logLevelHandle = null;

        public bool IsOutputLogLevel(KLogLevel level)
        {
            return null != logLevelHandle && logLevelHandle.logLevel <= level;
        }
    }


    public class KLogger
    {
        private const string LogLevelPathPrefix = "/Assets/";

        private static readonly Dictionary<string, KLogCategory> pathToCategoryDic = new Dictionary<string, KLogCategory>();
        private static readonly List<KLogLevelHandle> logLevelHandles = new List<KLogLevelHandle>();
        private static readonly KLogLevelHandle globalLogLevelHandle = new KLogLevelHandle();
        private static readonly StringBuilder stringBuilder = new StringBuilder(512);

#if UNITY_EDITOR
        public static bool IsDev = false;
#else
        public static bool IsDev = false;
#endif

#if UNITY_EDITOR
        public static bool LogCore = false;
#else
        public static bool LogCore = false;
#endif

        [Conditional("DEBUG")]
        public static void Log(string info, [CallerFilePath] string path = null, [CallerMemberName] string memberName = null, [CallerLineNumber] int lineNumber = 0)
        {
            KLogCategory logCategory = GetCategory(path);
            if (logCategory.IsOutputLogLevel(KLogLevel.Log))
            {
                UnityEngine.Debug.Log(FormatLog(info, logCategory.categoryName, memberName, lineNumber));
            }
        }

        public static void LogWarning(string info, [CallerFilePath] string path = null, [CallerMemberName] string memberName = null, [CallerLineNumber] int lineNumber = 0)
        {
            KLogCategory logCategory = GetCategory(path);
            if (logCategory.IsOutputLogLevel(KLogLevel.Warning))
            {
                UnityEngine.Debug.LogWarning(FormatLog(info, logCategory.categoryName, memberName, lineNumber));
            }
        }

        public static void LogError(string info, [CallerFilePath] string path = null, [CallerMemberName] string memberName = null, [CallerLineNumber] int lineNumber = 0)
        {
            KLogCategory logCategory = GetCategory(path);
            if (logCategory.IsOutputLogLevel(KLogLevel.Error))
            {
                UnityEngine.Debug.LogError(FormatLog(info, logCategory.categoryName, memberName, lineNumber));
            }
        }

        public static void LogException(Exception exception, [CallerFilePath] string path = null, [CallerMemberName] string memberName = null, [CallerLineNumber] int lineNumber = 0)
        {
            KLogCategory logCategory = GetCategory(path);
            if (logCategory.IsOutputLogLevel(KLogLevel.Exception))
            {
                UnityEngine.Debug.LogException(exception);
            }
        }

        public static void SetLogLevel(string path, KLogLevel level)
        {
            if (string.IsNullOrEmpty(path))
            {
                globalLogLevelHandle.logLevel = level;
                return;
            }

            KLogLevelHandle handle = GetLogLevelHandle(path);
            if (null == handle || handle.path != path)
            {
                KLogLevelHandle newHandle = new KLogLevelHandle();
                newHandle.path = path;
                logLevelHandles.Add(newHandle);
                UpdateLogLevelHandle(handle, newHandle);
                handle = newHandle;
            }

            LogWarning($"set log level: {path} -> {level}");
            handle.logLevel = level;
        }

        public static void SetDefaultLogLevel(KLogLevel level)
        {
            SetLogLevel(string.Empty, level);
        }

        public static void SetSelfLogLevel(KLogLevel level, [CallerFilePath] string path = null)
        {
            SetLogLevel(GetLogLevelPath(path), level);
        }

        public static void SetSelfDirectoryLogLevel(KLogLevel level, [CallerFilePath] string path = null)
        {
            string dir = AdjustPath(System.IO.Path.GetDirectoryName(GetLogLevelPath(path)));
            if (!dir.EndsWith("/"))
            {
                dir += "/";
            }

            SetLogLevel(dir, level);
        }

        private static string FormatLog(string logStr, string categoryName, string memberName, int lineNumber)
        {
            stringBuilder.Clear();
            stringBuilder.Append("[ ");
            stringBuilder.Append(categoryName);
            stringBuilder.Append(" ");
            stringBuilder.Append(memberName);
            stringBuilder.Append(":");
            stringBuilder.Append(lineNumber);
            stringBuilder.Append(" ] ");
            stringBuilder.Append(logStr);
            return stringBuilder.ToString();
        }

        private static KLogCategory GetCategory(string path)
        {
            KLogCategory logCategory = null;
            if (!pathToCategoryDic.TryGetValue(path, out logCategory))
            {
                logCategory = new KLogCategory();
                logCategory.logLevelPath = GetLogLevelPath(path);
                logCategory.categoryName = GetCategoryName(logCategory.logLevelPath);
                logCategory.logLevelHandle = GetLogLevelHandle(logCategory.logLevelPath);
                pathToCategoryDic.Add(path, logCategory);
                Log($"create log category: {path} -> {logCategory.logLevelPath}, {logCategory.categoryName}");
            }

            return logCategory;
        }

        private static KLogLevelHandle GetLogLevelHandle(string path)
        {
            if (string.IsNullOrEmpty(path))
            {
                return globalLogLevelHandle;
            }

            KLogLevelHandle foundHandle = globalLogLevelHandle;
            foreach (var handle in logLevelHandles)
            {
                if (foundHandle.path.Length < handle.path.Length && path.StartsWith(handle.path, System.StringComparison.Ordinal))
                {
                    foundHandle = handle;
                }
            }

            return foundHandle;
        }

        private static string GetCategoryName(string path)
        {
            if (string.IsNullOrEmpty(path))
            {
                return string.Empty;
            }

            return System.IO.Path.GetFileName(path);
        }

        private static void UpdateLogLevelHandle(KLogLevelHandle src, KLogLevelHandle dst)
        {
            if (null == src || null == dst)
            {
                return;
            }

            foreach (var kv in pathToCategoryDic)
            {
                KLogCategory logCategory = kv.Value;
                if (logCategory.logLevelHandle == src && logCategory.logLevelPath.StartsWith(dst.path, System.StringComparison.Ordinal))
                {
                    logCategory.logLevelHandle = dst;
                }
            }
        }

        private static string AdjustPath(string path)
        {
            if (string.IsNullOrEmpty(path))
            {
                return string.Empty;
            }

            return path.Replace("\\", "/");
        }

        private static string GetLogLevelPath(string srcPath)
        {
            string path = AdjustPath(srcPath);
            if (!path.StartsWith(LogLevelPathPrefix, System.StringComparison.Ordinal))
            {
                int index = path.IndexOf(LogLevelPathPrefix, System.StringComparison.Ordinal);
                if (index >= 0)
                {
                    path = path.Substring(index);
                }
            }

            return path;
        }
    }
}