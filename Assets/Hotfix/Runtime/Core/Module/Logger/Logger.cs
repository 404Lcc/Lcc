using System.Diagnostics;
using System;

namespace LccModel
{
    public interface ILog
    {
        void Trace(string message);
        void Debug(string message);
        void Info(string message);
        void Warning(string message);
        void Error(string message);
    }
    public class Logger : Singleton<Logger>
    {
        public ILog ILog { get; set; } = new UnityLogger();

        private const int TraceLevel = 1;
        private const int DebugLevel = 2;
        private const int InfoLevel = 3;
        private const int WarningLevel = 4;

        private bool CheckLogLevel(int level)
        {
            return true;
        }

        public void Trace(string message)
        {
            if (!CheckLogLevel(DebugLevel))
            {
                return;
            }
            StackTrace st = new StackTrace(2, true);
            ILog.Trace($"{message}\n{st}");
        }

        public void Debug(string message)
        {
            if (!CheckLogLevel(DebugLevel))
            {
                return;
            }
            ILog.Debug(message);
        }

        public void Info(string message)
        {
            if (!CheckLogLevel(InfoLevel))
            {
                return;
            }
            ILog.Info(message);
        }
        
        public void Warning(string message)
        {
            if (!CheckLogLevel(WarningLevel))
            {
                return;
            }

            ILog.Warning(message);
        }

        public void Error(string message)
        {
            StackTrace st = new StackTrace(2, true);
            ILog.Error($"{message}\n{st}");
        }

        public void Error(Exception e)
        {
            if (e.Data.Contains("StackTrace"))
            {
                ILog.Error($"{e.Data["StackTrace"]}\n{e}");
                return;
            }
            ILog.Error(e.ToString());
        }









        public void Trace(string message, params object[] args)
        {
            if (!CheckLogLevel(TraceLevel))
            {
                return;
            }
            StackTrace st = new StackTrace(2, true);
            ILog.Trace($"{string.Format(message, args)}\n{st}");
        }

        public void Debug(string message, params object[] args)
        {
            if (!CheckLogLevel(DebugLevel))
            {
                return;
            }
            ILog.Debug(string.Format(message, args));

        }


        public void Info(string message, params object[] args)
        {
            if (!CheckLogLevel(InfoLevel))
            {
                return;
            }
            ILog.Info(string.Format(message, args));
        }

        public void Warning(string message, params object[] args)
        {
            if (!CheckLogLevel(WarningLevel))
            {
                return;
            }
            ILog.Warning(string.Format(message, args));
        }


        public void Error(string message, params object[] args)
        {
            StackTrace st = new StackTrace(2, true);
            ILog.Error($"{string.Format(message, args)}\n{st}");
        }
    }
}