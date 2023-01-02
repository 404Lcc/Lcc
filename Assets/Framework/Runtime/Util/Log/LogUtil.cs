using System;
using System.Diagnostics;
using UnityEngine;

namespace LccModel
{
    public static class LogUtil
    {
        public static void Trace(string message)
        {
            if (Application.isEditor)
            {
                StackTrace st = new StackTrace(2, true);
                UnityEngine.Debug.Log($"{message}\n{st}");
                return;
            }
            Logger.Instance.Trace(message);
        }

        public static void Warning(string message)
        {
            if (Application.isEditor)
            {
                UnityEngine.Debug.LogWarning(message);
                return;
            }
            Logger.Instance.Warning(message);
        }

        public static void Info(string message)
        {
            if (Application.isEditor)
            {
                UnityEngine.Debug.Log(message);
                return;
            }
            Logger.Instance.Info(message);
        }

        public static void Debug(string message)
        {
            if (Application.isEditor)
            {
                UnityEngine.Debug.Log(message);
                return;
            }
            Logger.Instance.Debug(message);
        }

        public static void Error(string message)
        {
            if (Application.isEditor)
            {
                StackTrace st = new StackTrace(2, true);
                UnityEngine.Debug.LogError($"{message}\n{st}");
                return;
            }
            Logger.Instance.Error(message);
        }
        public static void Error(Exception e)
        {
            if (Application.isEditor)
            {
                if (e.Data.Contains("StackTrace"))
                {
                    UnityEngine.Debug.LogError($"{e.Data["StackTrace"]}\n{e}");
                    return;
                }
                UnityEngine.Debug.LogError(e.ToString());
                return;
            }
            Logger.Instance.Error(e);
        }




        public static void Trace(string message, params object[] args)
        {
            if (Application.isEditor)
            {
                StackTrace st = new StackTrace(2, true);
                UnityEngine.Debug.Log($"{string.Format(message, args)}\n{st}");
                return;
            }
            Logger.Instance.Trace(message, args);
        }

        public static void Warning(string message, params object[] args)
        {
            if (Application.isEditor)
            {
                UnityEngine.Debug.LogWarning(string.Format(message, args));
                return;
            }
            Logger.Instance.Warning(message, args);
        }

        public static void Info(string message, params object[] args)
        {
            if (Application.isEditor)
            {
                UnityEngine.Debug.Log(string.Format(message, args));
                return;
            }
            Logger.Instance.Info(message, args);
        }

        public static void Debug(string message, params object[] args)
        {
            if (Application.isEditor)
            {
                UnityEngine.Debug.Log(string.Format(message, args));
                return;
            }
            Logger.Instance.Debug(message, args);

        }

        public static void Error(string message, params object[] args)
        {
            if (Application.isEditor)
            {
                StackTrace st = new StackTrace(2, true);
                UnityEngine.Debug.LogError($"{string.Format(message, args)}\n{st}");
                return;
            }
            Logger.Instance.Error(message, args);
        }
    }
}