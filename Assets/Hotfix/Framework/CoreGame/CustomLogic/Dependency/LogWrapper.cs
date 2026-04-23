using System;
using System.IO;
using System.Text;
#if CONSOLE_CLIENT
using System.Diagnostics;
#else
using UnityEngine;
#endif

public enum LogPlatform
{
    Android,
    IOS,
    Win,
    Editor,
    WinEditor
}


public interface ILogWriter
{
    void WriteLog(string line);
}

public class LogFileWriter : ILogWriter
{
    protected const string PATH_END = "/";

    static LogPlatform _logPlatform = LogPlatform.Win;

    public void SetLogPlatform(LogPlatform platform)
    {
        _logPlatform = platform;
        _isInit = false;
    }

    public string GetLogDir()
    {
        switch (_logPlatform)
        {
            case LogPlatform.Android:
                return Application.persistentDataPath + PATH_END + "/" + LogWrapper.ProductPrefix + PATH_END;
            case LogPlatform.IOS:
                return Application.temporaryCachePath + "/" + LogWrapper.ProductPrefix + PATH_END;
            case LogPlatform.Win:
                return Application.dataPath + "/../" + LogWrapper.ProductPrefix + PATH_END;
            case LogPlatform.WinEditor:
                return Application.dataPath + "/../" + LogWrapper.ProductPrefix + PATH_END;
            case LogPlatform.Editor:
                return Application.dataPath + "/../" + LogWrapper.ProductPrefix + PATH_END;
        }

        return Application.dataPath + "/../" + LogWrapper.ProductPrefix + PATH_END;
    }

    /// <summary>
    /// 文件流
    /// </summary>
    private StreamWriter _streamWriter = null;

    /// <summary>
    /// 初始化标记，避免重复初始化
    /// </summary>
    private bool _isInit = false;

    const int MAX_LOG_ENTRY_NUM = 10;

    public void WriteLog(string line)
    {
        if (_streamWriter != null)
        {
            lock (_streamWriter)
            {
                try
                {
                    _streamWriter.WriteLine(line);
                }
                catch (System.Exception)
                {
                }
            }
        }
    }

    private static string[] GetLogFiles(string logFilePath)
    {
        return Directory.GetFiles(logFilePath, "*.txt", SearchOption.TopDirectoryOnly);
    }


    public void Init()
    {
        if (_isInit)
            return;

        try
        {
#if CONSOLE_CLIENT
            if (!Directory.Exists(LogWrapper.LogDir))
            {
                Directory.CreateDirectory(LogWrapper.LogDir);
            }
            string fileName = LogWrapper.LogDir + string.Format("{0}_{1:D2}_{2:D2}_{3:D2}_{4:D2}_{5:D2}.{6:D5}.{7}",
                DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day, DateTime.Now.Hour, DateTime.Now.Minute,
                DateTime.Now.Second, Process.GetCurrentProcess().Id, "log.txt");
#else
            string logDir = GetLogDir() + "log";


            if (!Directory.Exists(logDir))
            {
                Directory.CreateDirectory(logDir);
            }

            string[] logs = GetLogFiles(logDir);

            if (logs.Length > MAX_LOG_ENTRY_NUM)
            {
                foreach (var log in logs)
                {
                    File.Delete(log);
                }
            }

            string fileName = logDir + "/" + string.Format("{0}_{1}_{2}_{3}_{4}_{5}_{6}",
                DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day, DateTime.Now.Hour, DateTime.Now.Minute,
                DateTime.Now.Second, "log.txt");
#endif
            FileStream fs = new FileStream(fileName, FileMode.OpenOrCreate);
            _streamWriter = new StreamWriter(fs);
            _streamWriter.AutoFlush = true;
        }
        catch (System.Exception)
        {
            _streamWriter = null;
        }

        _isInit = true;
    }
}

public class LogWrapper
{
    public static string ProductPrefix = "GameCilent";
    public static string LogPrefix = ": " + ProductPrefix;
    static LogFileWriter _logFileWriter = null;
    static ILogWriter _guiWriter = null;


    public static void Init(LogPlatform platform)
    {
        _logFileWriter = new LogFileWriter();
        _logFileWriter.SetLogPlatform(platform);
        _logFileWriter.Init();
    }

    public enum ELogLevel
    {
        DEBUG = 800,
        INFO = 700,
        WARNING = 500,
        ERROR = 400,
        CRITICAL = 300,
    }

    private static ELogLevel _logLevel = ELogLevel.DEBUG;

    public static ELogLevel LogLevel
    {
        get { return _logLevel; }
        set { _logLevel = value; }
    }

#if CONSOLE_CLIENT
    public static String LogDir = "../logs/";
#else
#endif
    private static bool _enabled = true;

    public static ILogWriter GUIWriter
    {
        set { _guiWriter = value; }
    }

    public static bool Enabled
    {
        get { return _enabled; }
        set { _enabled = value; }
    }

    static bool CheckLogLevel(ELogLevel level)
    {
        if (_enabled && _logLevel >= level)
        {
            if (_logFileWriter != null)
                _logFileWriter.Init();
            return true;
        }

        return false;
    }

    private static string ToStrBuff(object[] data)
    {
        StringBuilder sb = new StringBuilder(256);
        for (int i = 0; i < data.Length; ++i)
        {
            sb.AppendFormat(" {0}", data[i]);
        }

        return sb.ToString();
    }

    private static void WriteLogLine(string levelname, ELogLevel level, string info)
    {
        if (!CheckLogLevel(level))
            return;

        //-----------------------
#if ConsoleClient
        string line = string.Format("{1} {2} {3} - {0}", info, DateTime.Now.ToString(), LogPrefix, levelname);
        Console.WriteLine( line );
#else
        string line = string.Format("{1} {2} {3} - {0}", info, DateTime.Now.ToString(), LogPrefix, levelname);
        Console.WriteLine(line);
        switch (level)
        {
            case ELogLevel.DEBUG:
                Debug.Log(line);
                break;
            case ELogLevel.INFO:
                Debug.Log(line);
                break;
            case ELogLevel.WARNING:
                Debug.LogWarning(line);
                break;
            case ELogLevel.ERROR:
                Debug.LogError(line);
                break;
            case ELogLevel.CRITICAL:
                Debug.LogError(line);
                break;
            default:
                Debug.Log(line);
                break;
        }
#endif
        //-----------------------
        if (_logFileWriter != null)
            _logFileWriter.WriteLog(line);

        if (_guiWriter != null)
        {
            _guiWriter.WriteLog(line);
        }
    }

    public static void LogDebug(params object[] data)
    {
        if (!CheckLogLevel(ELogLevel.DEBUG))
            return;

        LogDebug(ToStrBuff(data));
    }

    public static void LogDebug(string info)
    {
        WriteLogLine("Debug", ELogLevel.DEBUG, info);
    }

    public static void LogInfo(string info)
    {
        WriteLogLine("Info", ELogLevel.INFO, info);
    }

    public static void LogWarning(string info)
    {
        WriteLogLine("Warning", ELogLevel.WARNING, info);
    }

    public static void LogError(string info)
    {
        WriteLogLine("Error", ELogLevel.ERROR, info);
    }


    public static void LogCritical(string info)
    {
        WriteLogLine("Critical", ELogLevel.CRITICAL, info);
    }

    public static void Exception(Exception exp)
    {
        if (_logFileWriter != null)
            _logFileWriter.Init();

        WriteLogLine("Exception", ELogLevel.CRITICAL, exp.ToString());
    }

    public static void LogErrorDebugWrapper(string info)
    {
        string line = string.Format("{0}{1}{2}", "<color=#22BB00FF>DEBUG_ONLY: ", info, "</color>");
        WriteLogLine("Error", ELogLevel.ERROR, line);
    }

    public static void LogErrorDebugWrapper(params object[] data)
    {
        if (!CheckLogLevel(ELogLevel.ERROR))
            return;

        LogErrorDebugWrapper(ToStrBuff(data));
    }
}