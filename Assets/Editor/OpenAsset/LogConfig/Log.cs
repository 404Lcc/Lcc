using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEditor.Callbacks;

namespace LccEditor
{
    public static class Log
    {
        public static List<LogConfig> logConfigList = new List<LogConfig>();
        [OnOpenAsset]
        public static bool OnOpenAsset(int instanceId, int line)
        {
            //if (logConfigList.Count == 0)
            //{
            //    logConfigList.Add(new LogConfig("Assets/Framework/Runtime/Core/Module/Logger/UnityLogger.cs", typeof(UnityLogger).FullName));
            //}
            //string[] datas = GetStackTrace().Split('\n');
            //foreach (LogConfig item in logConfigList)
            //{
            //    if (item.instanceId == instanceId)
            //    {
            //        if (IsHotfix(datas, out int index))
            //        {
            //            string path = GetFilePath(datas[index]).Replace(UnityEngine.Application.dataPath.Replace("Assets", string.Empty), string.Empty);
            //            return AssetDatabase.OpenAsset(AssetDatabase.LoadAssetAtPath<Object>(path), GetFileLine(datas[index]));
            //        }
            //        else
            //        {
            //            string path = GetFilePath(datas[index]);
            //            return AssetDatabase.OpenAsset(AssetDatabase.LoadAssetAtPath<Object>(path), GetFileLine(datas[index]));
            //        }
            //    }
            //}
            return false;
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
        public static bool IsHotfix(string[] datas, out int index)
        {
            index = -1;
            for (int i = datas.Length - 1; i > 0; i--)
            {
                Regex regex = new Regex(@"IL_[\s\S]*[\s\S]*[\s\S]*[\s\S]*: call");
                Match match = regex.Match(datas[i]);
                if (match.Success)
                {
                    index = i + 3;
                    break;
                }
            }
            //不是热更
            if (index == -1)
            {
                for (int i = datas.Length - 1; i > 0; i--)
                {
                    bool isLog = false;
                    foreach (LogConfig item in logConfigList)
                    {
                        if (datas[i].Contains(item.name))
                        {
                            isLog = true;
                            break;
                        }
                    }
                    if (isLog)
                    {
                        index = i + 3;
                        break;
                    }
                }
                return false;
            }
            return true;
        }
        public static string GetFilePath(string data)
        {
            int start = data.IndexOf("(at ") + 4;
            int end = data.LastIndexOf(':');
            return data.Substring(start, end - start);
        }
        public static int GetFileLine(string data)
        {
            int start = data.LastIndexOf(':') + 1;
            int end = data.LastIndexOf(')');
            return int.Parse(data.Substring(start, end - start));
        }
    }
}