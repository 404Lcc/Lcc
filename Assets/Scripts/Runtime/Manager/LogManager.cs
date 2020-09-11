using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Model
{
    public class LogManager : Singleton<LogManager>
    {
        public Hashtable logs = new Hashtable();
        public override void OnEnable()
        {
            Application.logMessageReceived += HandleLog;
        }
        public override void OnDisable()
        {
            Application.logMessageReceived -= HandleLog;
        }
        private void HandleLog(string logString, string stackTrace, UnityEngine.LogType type)
        {
            if (type == UnityEngine.LogType.Error)
            {
            }
            if (type == UnityEngine.LogType.Log)
            {
            }
        }
        /// <summary>
        /// Log是否存在
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        private bool LogExist(LogType type)
        {
            if (logs.ContainsKey(type))
            {
                return true;
            }
            return false;
        }
        /// <summary>
        /// 创建Log
        /// </summary>
        /// <param name="type"></param>
        /// <param name="log"></param>
        /// <returns></returns>
        public LogInfo CreateLog(LogType type, string log)
        {
            LogInfo info = new LogInfo();
            info.state = InfoState.Close;
            info.container = ContainerManager.Instance.CreateContainer(GameUtil.ConvertLogTypeToString(type), false);
            info.type = type;
            info.logText = GameUtil.GetChildComponent<Text>(info.container, "Log");
            info.SetLog(log);
            info.ClosePanel();
            logs.Add(type, info);
            return info;
        }
        /// <summary>
        /// 删除Log
        /// </summary>
        /// <param name="type"></param>
        public void ClearLog(LogType type)
        {
            if (LogExist(type))
            {
                ContainerManager.Instance.RemoveContainer(GameUtil.ConvertLogTypeToString(type));
                LogInfo info = GetLogInfo(type);
                GameUtil.SafeDestroy(info.container);
                logs.Remove(type);
            }
        }
        /// <summary>
        /// 删除Log
        /// </summary>
        /// <param name="types"></param>
        /// <returns></returns>
        public void ClearLog(LogType[] types)
        {
            foreach (LogType item in types)
            {
                ClearLog(item);
            }
        }
        /// <summary>
        /// 删除剩下所有Log
        /// </summary>
        /// <param name="types"></param>
        /// <returns></returns>
        public void ClearExceptLog(LogType[] types)
        {
            List<LogType> typeList = new List<LogType>(types);
            foreach (LogType item in Enum.GetValues(typeof(LogType)))
            {
                if (!typeList.Contains(item))
                {
                    ClearLog(item);
                }
            }
        }
        /// <summary>
        /// 删除全部Log
        /// </summary>
        public void ClearAllLogs()
        {
            foreach (LogType item in Enum.GetValues(typeof(LogType)))
            {
                ClearLog(item);
            }
        }
        /// <summary>
        /// 删除全部打开的Log
        /// </summary>
        /// <returns></returns>
        public int ClearOpenLogs()
        {
            int number = 0;
            IDictionaryEnumerator enumerator = logs.GetEnumerator();
            while (enumerator.MoveNext())
            {
                LogInfo info = enumerator.Value as LogInfo;
                if (info.state == InfoState.Open)
                {
                    ClearLog(info.type);
                    number++;
                }
            }
            return number;
        }
        /// <summary>
        /// 打开Log
        /// </summary>
        /// <param name="type"></param>
        /// <param name="log"></param>
        /// <returns></returns>
        public LogInfo OpenLog(LogType type, string log)
        {
            if (LogExist(type))
            {
                LogInfo info = GetLogInfo(type);
                info.SetLog(log);
                info.OpenPanel();
                return info;
            }
            LogInfo temp = CreateLog(type, log);
            temp.OpenPanel();
            return temp;
        }
        /// <summary>
        /// 打开Log
        /// </summary>
        /// <param name="types"></param>
        /// <param name="info"></param>
        /// <returns></returns>
        public LogInfo[] OpenLog(LogType[] types, string info)
        {
            List<LogInfo> infos = new List<LogInfo>();
            foreach (LogType item in types)
            {
                infos.Add(OpenLog(item, info));
            }
            return infos.ToArray();
        }
        /// <summary>
        /// 打开剩下所有Log
        /// </summary>
        /// <param name="types"></param>
        /// <param name="info"></param>
        /// <returns></returns>
        public LogInfo[] OpenExceptLog(LogType[] types, string info)
        {
            List<LogInfo> infoList = new List<LogInfo>();
            List<LogType> typeList = new List<LogType>(types);
            foreach (LogType item in Enum.GetValues(typeof(LogType)))
            {
                if (!typeList.Contains(item))
                {
                    infoList.Add(OpenLog(item, info));
                }
            }
            return infoList.ToArray();
        }
        /// <summary>
        /// 打开全部Log
        /// </summary>
        /// <param name="info"></param>
        /// <returns></returns>
        public LogInfo[] OpenAllLogs(string info)
        {
            List<LogInfo> infoList = new List<LogInfo>();
            foreach (LogType item in Enum.GetValues(typeof(LogType)))
            {
                infoList.Add(OpenLog(item, info));
            }
            return infoList.ToArray();
        }
        /// <summary>
        /// 隐藏Log
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public LogInfo CloseLog(LogType type)
        {
            if (LogExist(type))
            {
                LogInfo info = GetLogInfo(type);
                info.ClosePanel();
                return info;
            }
            LogInfo temp = CreateLog(type, string.Empty);
            temp.ClosePanel();
            return temp;
        }
        /// <summary>
        /// 隐藏Log
        /// </summary>
        /// <param name="types"></param>
        /// <returns></returns>
        public LogInfo[] CloseLog(LogType[] types)
        {
            List<LogInfo> infoList = new List<LogInfo>();
            foreach (LogType item in types)
            {
                infoList.Add(CloseLog(item));
            }
            return infoList.ToArray();
        }
        /// <summary>
        /// 隐藏剩下所有Log
        /// </summary>
        /// <param name="types"></param>
        /// <returns></returns>
        public LogInfo[] CloseExceptLog(LogType[] types)
        {
            List<LogInfo> infoList = new List<LogInfo>();
            List<LogType> typelist = new List<LogType>(types);
            foreach (LogType item in Enum.GetValues(typeof(LogType)))
            {
                if (!typelist.Contains(item))
                {
                    infoList.Add(CloseLog(item));
                }
            }
            return infoList.ToArray();
        }
        /// <summary>
        /// 隐藏全部Log
        /// </summary>
        public LogInfo[] CloseAllLogs()
        {
            List<LogInfo> infoList = new List<LogInfo>();
            foreach (LogType item in Enum.GetValues(typeof(LogType)))
            {
                infoList.Add(CloseLog(item));
            }
            return infoList.ToArray();
        }
        /// <summary>
        /// 获取Log
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public LogInfo GetLogInfo(LogType type)
        {
            if (LogExist(type))
            {
                LogInfo info = logs[type] as LogInfo;
                return info;
            }
            return null;
        }
        /// <summary>
        /// Log是否打开
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public bool IsOpenLog(LogType type)
        {
            if (LogExist(type))
            {
                LogInfo info = GetLogInfo(type);
                if (info.state == InfoState.Open)
                {
                    return true;
                }
                return false;
            }
            return false;
        }
    }
}