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


        void Trace(string message, params object[] args);
        void Debug(string message, params object[] args);
        void Info(string message, params object[] args);
        void Warning(string message, params object[] args);
        void Error(string message, params object[] args);
    }
    public class Logger : Singleton<Logger>
    {
        private ILog iLog;

        public ILog ILog
        {
            set
            {
                iLog = value;
            }
        }

        private const int TraceLevel = 1;
        private const int DebugLevel = 2;
        private const int InfoLevel = 3;
        private const int WarningLevel = 4;

        private bool CheckLogLevel(int level)
        {
            return true;
        }

        public void Trace(string msg)
        {
            if (!CheckLogLevel(DebugLevel))
            {
                return;
            }
            StackTrace st = new StackTrace(2, true);
            this.iLog.Trace($"{msg}\n{st}");
        }

        public void Debug(string msg)
        {
            if (!CheckLogLevel(DebugLevel))
            {
                return;
            }
            this.iLog.Debug(msg);
        }

        public void Info(string msg)
        {
            if (!CheckLogLevel(InfoLevel))
            {
                return;
            }
            this.iLog.Info(msg);
        }
        
        public void Warning(string msg)
        {
            if (!CheckLogLevel(WarningLevel))
            {
                return;
            }

            this.iLog.Warning(msg);
        }

        public void Error(string msg)
        {
            StackTrace st = new StackTrace(2, true);
            this.iLog.Error($"{msg}\n{st}");
        }

        public void Error(Exception e)
        {
            if (e.Data.Contains("StackTrace"))
            {
                this.iLog.Error($"{e.Data["StackTrace"]}\n{e}");
                return;
            }
            string str = e.ToString();
            this.iLog.Error(str);
        }









        public void Trace(string message, params object[] args)
        {
            if (!CheckLogLevel(TraceLevel))
            {
                return;
            }
            StackTrace st = new StackTrace(2, true);
            this.iLog.Trace($"{string.Format(message, args)}\n{st}");
        }

        public void Debug(string message, params object[] args)
        {
            if (!CheckLogLevel(DebugLevel))
            {
                return;
            }
            this.iLog.Debug(string.Format(message, args));

        }


        public void Info(string message, params object[] args)
        {
            if (!CheckLogLevel(InfoLevel))
            {
                return;
            }
            this.iLog.Info(string.Format(message, args));
        }

        public void Warning(string message, params object[] args)
        {
            if (!CheckLogLevel(WarningLevel))
            {
                return;
            }
            this.iLog.Warning(string.Format(message, args));
        }


        public void Error(string message, params object[] args)
        {
            StackTrace st = new StackTrace(2, true);
            string s = string.Format(message, args) + '\n' + st;
            this.iLog.Error(s);
        }
    }
}