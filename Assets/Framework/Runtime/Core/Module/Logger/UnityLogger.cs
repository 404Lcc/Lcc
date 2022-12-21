namespace LccModel
{
    public class UnityLogger : ILog
    {
        public void Trace(string message)
        {
            UnityEngine.Debug.Log(message);
        }

        public void Debug(string message)
        {
            UnityEngine.Debug.Log(message);
        }

        public void Info(string message)
        {
            UnityEngine.Debug.Log(message);
        }

        public void Warning(string message)
        {
            UnityEngine.Debug.LogWarning(message);
        }

        public void Error(string message)
        {
            UnityEngine.Debug.LogError(message);
        }







        public void Trace(string message, params object[] args)
        {
            UnityEngine.Debug.LogFormat(message, args);
        }

        public void Debug(string message, params object[] args)
        {
            UnityEngine.Debug.LogFormat(message, args);
        }

        public void Info(string message, params object[] args)
        {
            UnityEngine.Debug.LogFormat(message, args);
        }

        public void Warning(string message, params object[] args)
        {
            UnityEngine.Debug.LogWarningFormat(message, args);
        }

        public void Error(string message, params object[] args)
        {
            UnityEngine.Debug.LogErrorFormat(message, args);
        }
    }
}