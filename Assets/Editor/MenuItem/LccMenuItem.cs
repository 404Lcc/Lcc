using Model;
using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

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
        string locationPathName = GameUtil.GetPath(PathType.PersistentDataPath, "Build") + name;
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
            AssetDatabase.Refresh();
        }
#endif
    }
}