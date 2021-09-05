using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEditor;
using UnityEditor.Callbacks;
using Object = UnityEngine.Object;

namespace LccEditor
{
    public static class LogConfigUtil
    {
        public static List<LogConfig> logConfigList = new List<LogConfig>();
        [OnOpenAsset]
        public static bool OnOpenAsset(int instanceId, int line)
        {
            InitLogConfigList();
            InitInstanceId();
            foreach (LogConfig item in logConfigList)
            {
                if (item.instanceId == instanceId)
                {
                    string stackTrace = GetStackTrace();
                    if (!string.IsNullOrEmpty(stackTrace))
                    {
                        string[] fileNames = stackTrace.Split('\n');
                        string fileName = GetFileName(fileNames);
                        AssetDatabase.OpenAsset(AssetDatabase.LoadAssetAtPath<Object>(GetFileName(fileName)), GetFileLine(fileName));
                        return true;
                    }
                }
            }
            return false;
        }
        public static void InitLogConfigList()
        {
            if (logConfigList.Count == 0)
            {
                logConfigList.Add(new LogConfig("Assets/Scripts/Runtime/Core/Util/Log/LogUtil.cs", typeof(LccModel.LogUtil).FullName));
            }
        }
        public static void InitInstanceId()
        {
            foreach (LogConfig item in logConfigList)
            {
                if (item.instanceId > 0)
                {
                    return;
                }
                Object asset = AssetDatabase.LoadAssetAtPath<Object>(item.path);
                item.instanceId = asset.GetInstanceID();
            }
        }
        public static string GetStackTrace()
        {
            Type consoleWindowType = typeof(EditorWindow).Assembly.GetType("UnityEditor.ConsoleWindow");
            FieldInfo fieldInfo = consoleWindowType.GetField("ms_ConsoleWindow", BindingFlags.Static | BindingFlags.NonPublic);
            object obj = fieldInfo.GetValue(null);
            if (obj != null)
            {
                if (obj == (object)EditorWindow.focusedWindow)
                {
                    fieldInfo = consoleWindowType.GetField("m_ActiveText", BindingFlags.Instance | BindingFlags.NonPublic);
                    return fieldInfo.GetValue(obj).ToString();
                }
            }
            return string.Empty;
        }
        public static string GetFileName(string[] fileNames)
        {
            int index = -1;
            for (int i = fileNames.Length - 1; i > 0; i--)
            {
                bool isLog = false;
                foreach (LogConfig item in logConfigList)
                {
                    if (fileNames[i].Contains(item.name))
                    {
                        isLog = true;
                        break;
                    }
                }
                if (isLog)
                {
                    index = i + 1;
                    break;
                }
            }
            return fileNames[index];
        }
        public static string GetFileName(string fileName)
        {
            int start = fileName.IndexOf("(at ") + 4;
            int end = fileName.LastIndexOf(':');
            return fileName.Substring(start, end - start);
        }
        public static int GetFileLine(string fileName)
        {
            int start = fileName.LastIndexOf(':') + 1;
            int end = fileName.LastIndexOf(')');
            return int.Parse(fileName.Substring(start, end - start));
        }
    }
}