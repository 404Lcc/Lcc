using System;

namespace LccModel
{
    public static class LogUtil
    {
        public static void Trace(string message)
        {
            Logger.Instance.Trace(message);
        }

        public static void Warning(string message)
        {
            Logger.Instance.Warning(message);
        }

        public static void Info(string message)
        {
            Logger.Instance.Info(message);
        }

        public static void Debug(string message)
        {
            Logger.Instance.Debug(message);
        }

        public static void Error(string message)
        {
            Logger.Instance.Error(message);
        }
        public static void Error(Exception e)
        {
            Logger.Instance.Error(e);
        }




        public static void Trace(string message, params object[] args)
        {
            Logger.Instance.Trace(message, args);
        }

        public static void Warning(string message, params object[] args)
        {
            Logger.Instance.Warning(message, args);
        }

        public static void Info(string message, params object[] args)
        {
            Logger.Instance.Info(message, args);
        }

        public static void Debug(string message, params object[] args)
        {
            Logger.Instance.Debug(message, args);

        }

        public static void Error(string message, params object[] args)
        {
            Logger.Instance.Error(message, args);
        }
    }
}