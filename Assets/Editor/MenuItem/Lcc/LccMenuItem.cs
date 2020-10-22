using LccModel;
using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace LccEditor
{
    public class LccMenuItem
    {
        [MenuItem("Lcc/ViewPersistentData")]
        private static void ViewPersistentData()
        {
            EditorUtility.OpenWithDefaultApp(Application.persistentDataPath);
        }
        [MenuItem("Lcc/BuildPlayer")]
        private static void BuildPlayer()
        {
            List<string> argList = new List<string>();
            foreach (string item in Environment.GetCommandLineArgs())
            {
                argList.Add(item);
            }
            string name = PlayerSettings.productName + " v" + PlayerSettings.bundleVersion;
            switch (EditorUserBuildSettings.activeBuildTarget)
            {
                case BuildTarget.StandaloneWindows:
                    name += ".exe";
                    break;
                case BuildTarget.Android:
                    name += ".apk";
                    break;
            }
            string locationPathName = PathUtil.GetPath(PathType.PersistentDataPath, "Build") + name;
            BuildPipeline.BuildPlayer(EditorBuildSettings.scenes, locationPathName, EditorUserBuildSettings.activeBuildTarget, EditorUserBuildSettings.development ? BuildOptions.Development : BuildOptions.None);
        }
        [MenuItem("Lcc/ILRuntime")]
        private static void ILRuntime()
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
        [MenuItem("Lcc/Mono")]
        private static void Mono()
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
                    define += item + ";";
                }
                PlayerSettings.SetScriptingDefineSymbolsForGroup(buildTargetGroup, define);
            }
        }
        [MenuItem("Lcc/AssetBundle")]
        private static void AssetBundle()
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
        [MenuItem("Lcc/Resources")]
        private static void Resources()
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
                    define += item + ";";
                }
                PlayerSettings.SetScriptingDefineSymbolsForGroup(buildTargetGroup, define);
            }
        }
        [MenuItem("Lcc/Release")]
        private static void Release()
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
        [MenuItem("Lcc/Debug")]
        private static void Debug()
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
                    define += item + ";";
                }
                PlayerSettings.SetScriptingDefineSymbolsForGroup(buildTargetGroup, define);
            }
        }
        [MenuItem("Assets/Lcc/BuildRelease")]
        private static void BuildRelease()
        {
#if Release
        if (File.Exists("Assets/Resources/Text/Unity.Hotfix.dll.bytes"))
        {
            File.Delete("Assets/Resources/Text/Unity.Hotfix.dll.bytes");
        }
        if (File.Exists("Assets/Resources/Text/Unity.Hotfix.pdb.bytes"))
        {
            File.Delete("Assets/Resources/Text/Unity.Hotfix.pdb.bytes");
        }
        if (File.Exists("Temp/bin/Release/Unity.Hotfix.dll"))
        {
            File.Copy("Temp/bin/Release/Unity.Hotfix.dll", "Assets/Resources/Text/Unity.Hotfix.dll.bytes", true);
            GameUtil.SaveAsset("Assets/Resources/Text/Unity.Hotfix.dll.bytes", GameUtil.RijndaelEncrypt("xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx", GameUtil.GetAsset("Assets/Resources/Text/Unity.Hotfix.dll.bytes")));
            AssetDatabase.Refresh();
        }
#endif
        }
        [MenuItem("Assets/Lcc/Create/Hotfix/Panel")]
        private static void CreateHotfixPanel()
        {
            string pathName = CreateScriptAction.GetSelectedPath() + "/NewHotfixPanel.cs";
            Texture2D icon = (Texture2D)EditorGUIUtility.IconContent("cs Script Icon").image;
            ProjectWindowUtil.StartNameEditingIfProjectWindowExists(0, ScriptableObject.CreateInstance<CreateScriptAction>(), pathName, icon, "Assets/Editor/Model/NewHotfixPanel.cs");
        }
        [MenuItem("Assets/Lcc/Create/Hotfix/ViewModel")]
        private static void CreateHotfixViewModel()
        {
            string pathName = CreateScriptAction.GetSelectedPath() + "/NewHotfixViewModel.cs";
            Texture2D icon = (Texture2D)EditorGUIUtility.IconContent("cs Script Icon").image;
            ProjectWindowUtil.StartNameEditingIfProjectWindowExists(0, ScriptableObject.CreateInstance<CreateScriptAction>(), pathName, icon, "Assets/Editor/Model/NewHotfixViewModel.cs");
        }
        [MenuItem("Assets/Lcc/Create/Model/Panel")]
        private static void CreateModelPanel()
        {
            string pathName = CreateScriptAction.GetSelectedPath() + "/NewModelPanel.cs";
            Texture2D icon = (Texture2D)EditorGUIUtility.IconContent("cs Script Icon").image;
            ProjectWindowUtil.StartNameEditingIfProjectWindowExists(0, ScriptableObject.CreateInstance<CreateScriptAction>(), pathName, icon, "Assets/Editor/Model/NewModelPanel.cs");
        }
        [MenuItem("Assets/Lcc/Create/Model/ViewModel")]
        private static void CreateModelViewModel()
        {
            string pathName = CreateScriptAction.GetSelectedPath() + "/NewModelViewModel.cs";
            Texture2D icon = (Texture2D)EditorGUIUtility.IconContent("cs Script Icon").image;
            ProjectWindowUtil.StartNameEditingIfProjectWindowExists(0, ScriptableObject.CreateInstance<CreateScriptAction>(), pathName, icon, "Assets/Editor/Model/NewModelViewModel.cs");
        }
    }
}