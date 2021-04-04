using Sirenix.OdinInspector;
using System.Collections.Generic;
using UnityEditor;

namespace LccEditor
{
    public class AssetEditorWindowBase : AEditorWindowBase
    {
        [PropertySpace(10)]
        [HideLabel, DisplayAsString]
        public string info;
        public AssetEditorWindowBase()
        {
        }
        public AssetEditorWindowBase(EditorWindow editorWindow) : base(editorWindow)
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
            if (defineList.Contains("AssetBundle"))
            {
                info = "当前是AssetBundle模式";
            }
            else
            {
                info = "当前是Resources模式";
            }
        }
        [PropertySpace(10)]
        [LabelText("AssetBundle模式"), Button]
        public void AssetBundle()
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
            if (!defineList.Contains("AssetBundle"))
            {
                define += ";AssetBundle";
                PlayerSettings.SetScriptingDefineSymbolsForGroup(buildTargetGroup, define);
            }
        }
        [PropertySpace(10)]
        [LabelText("Resources模式"), Button]
        public void Resources()
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
            if (defineList.Contains("AssetBundle"))
            {
                defineList.Remove("AssetBundle");
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