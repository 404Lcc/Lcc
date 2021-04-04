using Sirenix.OdinInspector;
using System.Collections.Generic;
using UnityEditor;

namespace LccEditor
{
    public class HotfixEditorWindowBase : AEditorWindowBase
    {
        [PropertySpace(10)]
        [HideLabel, DisplayAsString]
        public string info;
        public HotfixEditorWindowBase()
        {
        }
        public HotfixEditorWindowBase(EditorWindow editorWindow) : base(editorWindow)
        {
            BuildTargetGroup buildTargetGroup;
#if UNITY_STANDALONE
            buildTargetGroup = BuildTargetGroup.Standalone;
#endif
#if UNITY_ANDROID
            buildTargetGroup = BuildTargetGroup.Android;
#endif
#if UNITY_IOS
            buildTargetGroup = BuildTargetGroup.iOS;
#endif
            string define = PlayerSettings.GetScriptingDefineSymbolsForGroup(buildTargetGroup);
            List<string> defineList = new List<string>(define.Split(';'));
            if (defineList.Contains("ILRuntime"))
            {
                info = "当前是ILRuntime模式";
            }
            else
            {
                info = "当前是Mono模式";
            }
            if (defineList.Contains("Release"))
            {
                info += "Release的DLL";
            }
            else
            {
                info += "Debug的DLL";
            }
        }
        [PropertySpace(10)]
        [LabelText("ILRuntime模式"), Button]
        public void ILRuntime()
        {
            BuildTargetGroup buildTargetGroup;
#if UNITY_STANDALONE
            buildTargetGroup = BuildTargetGroup.Standalone;
#endif
#if UNITY_ANDROID
            buildTargetGroup = BuildTargetGroup.Android;
#endif
#if UNITY_IOS
            buildTargetGroup = BuildTargetGroup.iOS;
#endif
            string define = PlayerSettings.GetScriptingDefineSymbolsForGroup(buildTargetGroup);
            List<string> defineList = new List<string>(define.Split(';'));
            if (!defineList.Contains("ILRuntime"))
            {
                define += ";ILRuntime";
                PlayerSettings.SetScriptingDefineSymbolsForGroup(buildTargetGroup, define);
            }
        }
        [PropertySpace(10)]
        [LabelText("Mono模式"), Button]
        public void Mono()
        {
            BuildTargetGroup buildTargetGroup;
#if UNITY_STANDALONE
            buildTargetGroup = BuildTargetGroup.Standalone;
#endif
#if UNITY_ANDROID
            buildTargetGroup = BuildTargetGroup.Android;
#endif
#if UNITY_IOS
            buildTargetGroup = BuildTargetGroup.iOS;
#endif
            string define = PlayerSettings.GetScriptingDefineSymbolsForGroup(buildTargetGroup);
            List<string> defineList = new List<string>(define.Split(';'));
            if (defineList.Contains("ILRuntime"))
            {
                defineList.Remove("ILRuntime");
                define = string.Empty;
                foreach (string item in defineList)
                {
                    define += $"{item};";
                }
                PlayerSettings.SetScriptingDefineSymbolsForGroup(buildTargetGroup, define);
            }
        }
        [PropertySpace(10)]
        [LabelText("Release模式"), Button]
        public void Release()
        {
            BuildTargetGroup buildTargetGroup;
#if UNITY_STANDALONE
            buildTargetGroup = BuildTargetGroup.Standalone;
#endif
#if UNITY_ANDROID
            buildTargetGroup = BuildTargetGroup.Android;
#endif
#if UNITY_IOS
            buildTargetGroup = BuildTargetGroup.iOS;
#endif
            string define = PlayerSettings.GetScriptingDefineSymbolsForGroup(buildTargetGroup);
            List<string> defineList = new List<string>(define.Split(';'));
            if (!defineList.Contains("Release"))
            {
                define += ";Release";
                PlayerSettings.SetScriptingDefineSymbolsForGroup(buildTargetGroup, define);
            }
        }
        [PropertySpace(10)]
        [LabelText("Debug模式"), Button]
        public void Debug()
        {
            BuildTargetGroup buildTargetGroup;
#if UNITY_STANDALONE
            buildTargetGroup = BuildTargetGroup.Standalone;
#endif
#if UNITY_ANDROID
            buildTargetGroup = BuildTargetGroup.Android;
#endif
#if UNITY_IOS
            buildTargetGroup = BuildTargetGroup.iOS;
#endif
            string define = PlayerSettings.GetScriptingDefineSymbolsForGroup(buildTargetGroup);
            List<string> defineList = new List<string>(define.Split(';'));
            if (defineList.Contains("Release"))
            {
                defineList.Remove("Release");
                define = string.Empty;
                foreach (string item in defineList)
                {
                    define += $"{item};";
                }
                PlayerSettings.SetScriptingDefineSymbolsForGroup(buildTargetGroup, define);
            }
        }
    }
}