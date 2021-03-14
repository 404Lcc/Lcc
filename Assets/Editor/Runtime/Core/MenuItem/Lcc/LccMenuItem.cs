using LccModel;
using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;
using FileUtil = LccModel.FileUtil;

namespace LccEditor
{
    public class LccMenuItem
    {
        [MenuItem("Lcc/ViewPersistentData")]
        public static void ViewPersistentData()
        {
            EditorUtility.OpenWithDefaultApp(Application.persistentDataPath);
        }
        [MenuItem("Lcc/BuildPlayer")]
        public static void BuildPlayer()
        {
            List<string> argList = new List<string>();
            foreach (string item in Environment.GetCommandLineArgs())
            {
                argList.Add(item);
            }
            string name = $"{PlayerSettings.productName} v{PlayerSettings.bundleVersion}";
#if UNITY_STANDALONE
            name = $"{name}.exe";
#endif
#if UNITY_ANDROID
            name = $"{name}.apk";
#endif
            string locationPathName = $"{PathUtil.GetPath(PathType.PersistentDataPath, "Build")}/{name}";
            BuildPipeline.BuildPlayer(EditorBuildSettings.scenes, locationPathName, EditorUserBuildSettings.activeBuildTarget, EditorUserBuildSettings.development ? BuildOptions.Development : BuildOptions.None);
        }
        [MenuItem("Assets/Lcc/TagFileRule")]
        public static void TagFileRule()
        {
            if (File.Exists("Assets/Editor/Runtime/Core/Util/AssetBundle/AssetBundleSetting.asset"))
            {
                AssetBundleSetting assetBundleSetting = AssetDatabase.LoadAssetAtPath<AssetBundleSetting>("Assets/Editor/Runtime/Core/Util/AssetBundle/AssetBundleSetting.asset");
                if (assetBundleSetting.assetBundleRuleList == null)
                {
                    assetBundleSetting.assetBundleRuleList = new List<AssetBundleRule>();
                }
                assetBundleSetting.assetBundleRuleList.Add(AssetBundleUtil.TagFileRule());
                assetBundleSetting.assetBundleDataList = AssetBundleUtil.BuildAssetBundleData(assetBundleSetting.assetBundleRuleList.ToArray());
            }
            else
            {
                AssetBundleSetting assetBundleSetting = ScriptableObject.CreateInstance<AssetBundleSetting>();
                assetBundleSetting.assetBundleRuleList = new List<AssetBundleRule>();
                assetBundleSetting.assetBundleRuleList.Add(AssetBundleUtil.TagFileRule());
                assetBundleSetting.assetBundleDataList = AssetBundleUtil.BuildAssetBundleData(assetBundleSetting.assetBundleRuleList.ToArray());
                AssetDatabase.CreateAsset(assetBundleSetting, "Assets/Editor/Runtime/Core/Util/AssetBundle/AssetBundleSetting.asset");
                AssetDatabase.Refresh();
            }
        }
        [MenuItem("Assets/Lcc/TagDirectoryRule")]
        public static void TagDirectoryRule()
        {
            if (File.Exists("Assets/Editor/Runtime/Core/Util/AssetBundle/AssetBundleSetting.asset"))
            {
                AssetBundleSetting assetBundleSetting = AssetDatabase.LoadAssetAtPath<AssetBundleSetting>("Assets/Editor/Runtime/Core/Util/AssetBundle/AssetBundleSetting.asset");
                if (assetBundleSetting.assetBundleRuleList == null)
                {
                    assetBundleSetting.assetBundleRuleList = new List<AssetBundleRule>();
                }
                assetBundleSetting.assetBundleRuleList.Add(AssetBundleUtil.TagDirectoryRule());
                assetBundleSetting.assetBundleDataList = AssetBundleUtil.BuildAssetBundleData(assetBundleSetting.assetBundleRuleList.ToArray());
            }
            else
            {
                AssetBundleSetting assetBundleSetting = ScriptableObject.CreateInstance<AssetBundleSetting>();
                assetBundleSetting.assetBundleRuleList = new List<AssetBundleRule>();
                assetBundleSetting.assetBundleRuleList.Add(AssetBundleUtil.TagDirectoryRule());
                assetBundleSetting.assetBundleDataList = AssetBundleUtil.BuildAssetBundleData(assetBundleSetting.assetBundleRuleList.ToArray());
                AssetDatabase.CreateAsset(assetBundleSetting, "Assets/Editor/Runtime/Core/Util/AssetBundle/AssetBundleSetting.asset");
                AssetDatabase.Refresh();
            }
        }
        [MenuItem("Assets/Lcc/BuildAssetBundle")]
        public static void BuildAssetBundle()
        {
            if (File.Exists("Assets/Editor/Runtime/Core/Util/AssetBundle/AssetBundleSetting.asset"))
            {
                AssetBundleSetting assetBundleSetting = AssetDatabase.LoadAssetAtPath<AssetBundleSetting>("Assets/Editor/Runtime/Core/Util/AssetBundle/AssetBundleSetting.asset");
                assetBundleSetting.buildId++;
                if (string.IsNullOrEmpty(assetBundleSetting.outputPath))
                {
                    assetBundleSetting.outputPath = "Assets/AssetBundles";
                }
                AssetBundleUtil.BuildAssetBundle(assetBundleSetting);
            }
        }
        [MenuItem("Lcc/ILRuntime")]
        public static void ILRuntime()
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
        public static void Mono()
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
        [MenuItem("Lcc/AssetBundle")]
        public static void AssetBundle()
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
        public static void Resources()
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
        [MenuItem("Lcc/Release")]
        public static void Release()
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
        public static void Debug()
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
        [MenuItem("Lcc/ExcelExport")]
        public static void ExcelExport()
        {
            ExcelExportUtil.ExportAll();
        }
        [MenuItem("Assets/Lcc/Create/Hotfix/Panel")]
        public static void CreateHotfixPanel()
        {
            string pathName = $"{CreateScriptUtil.GetSelectedPath()}/NewHotfixPanel.cs";
            Texture2D icon = (Texture2D)EditorGUIUtility.IconContent("cs Script Icon").image;
            ProjectWindowUtil.StartNameEditingIfProjectWindowExists(0, ScriptableObject.CreateInstance<CreateScriptUtil>(), pathName, icon, "Assets/Editor/Runtime/Core/Model/NewHotfixPanel.cs");
        }
        [MenuItem("Assets/Lcc/Create/Hotfix/ViewModel")]
        public static void CreateHotfixViewModel()
        {
            string pathName = $"{CreateScriptUtil.GetSelectedPath()}/NewHotfixViewModel.cs";
            Texture2D icon = (Texture2D)EditorGUIUtility.IconContent("cs Script Icon").image;
            ProjectWindowUtil.StartNameEditingIfProjectWindowExists(0, ScriptableObject.CreateInstance<CreateScriptUtil>(), pathName, icon, "Assets/Editor/Runtime/Core/Model/NewHotfixViewModel.cs");
        }
        [MenuItem("Assets/Lcc/Create/Model/Panel")]
        public static void CreateModelPanel()
        {
            string pathName = $"{CreateScriptUtil.GetSelectedPath()}/NewModelPanel.cs";
            Texture2D icon = (Texture2D)EditorGUIUtility.IconContent("cs Script Icon").image;
            ProjectWindowUtil.StartNameEditingIfProjectWindowExists(0, ScriptableObject.CreateInstance<CreateScriptUtil>(), pathName, icon, "Assets/Editor/Runtime/Core/Model/NewModelPanel.cs");
        }
        [MenuItem("Assets/Lcc/Create/Model/ViewModel")]
        public static void CreateModelViewModel()
        {
            string pathName = $"{CreateScriptUtil.GetSelectedPath()}/NewModelViewModel.cs";
            Texture2D icon = (Texture2D)EditorGUIUtility.IconContent("cs Script Icon").image;
            ProjectWindowUtil.StartNameEditingIfProjectWindowExists(0, ScriptableObject.CreateInstance<CreateScriptUtil>(), pathName, icon, "Assets/Editor/Runtime/Core/Model/NewModelViewModel.cs");
        }
    }
}