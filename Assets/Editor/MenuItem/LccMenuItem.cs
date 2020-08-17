using Model;
using System;
using System.Collections.Generic;
using System.IO;
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
        List<string> arglist = new List<string>();
        foreach (string item in Environment.GetCommandLineArgs())
        {
            arglist.Add(item);
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
        string locationpathname = GameUtil.GetPath(PathType.PersistentDataPath, "Build") + name;
        BuildPipeline.BuildPlayer(EditorBuildSettings.scenes, locationpathname, EditorUserBuildSettings.activeBuildTarget, EditorUserBuildSettings.development ? BuildOptions.Development : BuildOptions.None);
    }
    [MenuItem("Lcc/ILRuntime")]
    private static void ILRuntime()
    {
        BuildTargetGroup buildtargetgroup;
#if UNITY_STANDALONE
        buildtargetgroup = BuildTargetGroup.Standalone;
#endif
#if UNITY_ANDROID
        buildtargetgroup = BuildTargetGroup.Android;
#endif
#if UNITY_IOS
        buildtargetgroup = BuildTargetGroup.iOS;
#endif
        string define = PlayerSettings.GetScriptingDefineSymbolsForGroup(buildtargetgroup);
        List<string> definelist = new List<string>(define.Split(';'));
        if (!definelist.Contains("ILRuntime"))
        {
            define += ";ILRuntime";
            PlayerSettings.SetScriptingDefineSymbolsForGroup(buildtargetgroup, define);
        }
    }
    [MenuItem("Lcc/Mono")]
    private static void Mono()
    {
        BuildTargetGroup buildtargetgroup;
#if UNITY_STANDALONE
        buildtargetgroup = BuildTargetGroup.Standalone;
#endif
#if UNITY_ANDROID
        buildtargetgroup = BuildTargetGroup.Android;
#endif
#if UNITY_IOS
        buildtargetgroup = BuildTargetGroup.iOS;
#endif
        string define = PlayerSettings.GetScriptingDefineSymbolsForGroup(buildtargetgroup);
        List<string> definelist = new List<string>(define.Split(';'));
        if (definelist.Contains("ILRuntime"))
        {
            definelist.Remove("ILRuntime");
            define = string.Empty;
            foreach (string item in definelist)
            {
                define += item + ";";
            }
            PlayerSettings.SetScriptingDefineSymbolsForGroup(buildtargetgroup, define);
        }
    }
    [MenuItem("Lcc/AssetBundle")]
    private static void AssetBundle()
    {
        BuildTargetGroup buildtargetgroup;
#if UNITY_STANDALONE
        buildtargetgroup = BuildTargetGroup.Standalone;
#endif
#if UNITY_ANDROID
        buildtargetgroup = BuildTargetGroup.Android;
#endif
#if UNITY_IOS
        buildtargetgroup = BuildTargetGroup.iOS;
#endif
        string define = PlayerSettings.GetScriptingDefineSymbolsForGroup(buildtargetgroup);
        List<string> definelist = new List<string>(define.Split(';'));
        if (!definelist.Contains("AssetBundle"))
        {
            define += ";AssetBundle";
            PlayerSettings.SetScriptingDefineSymbolsForGroup(buildtargetgroup, define);
        }
    }
    [MenuItem("Lcc/Resources")]
    private static void Resources()
    {
        BuildTargetGroup buildtargetgroup;
#if UNITY_STANDALONE
        buildtargetgroup = BuildTargetGroup.Standalone;
#endif
#if UNITY_ANDROID
        buildtargetgroup = BuildTargetGroup.Android;
#endif
#if UNITY_IOS
        buildtargetgroup = BuildTargetGroup.iOS;
#endif
        string define = PlayerSettings.GetScriptingDefineSymbolsForGroup(buildtargetgroup);
        List<string> definelist = new List<string>(define.Split(';'));
        if (definelist.Contains("AssetBundle"))
        {
            definelist.Remove("AssetBundle");
            define = string.Empty;
            foreach (string item in definelist)
            {
                define += item + ";";
            }
            PlayerSettings.SetScriptingDefineSymbolsForGroup(buildtargetgroup, define);
        }
    }
    [MenuItem("Lcc/Release")]
    private static void Release()
    {
        BuildTargetGroup buildtargetgroup;
#if UNITY_STANDALONE
        buildtargetgroup = BuildTargetGroup.Standalone;
#endif
#if UNITY_ANDROID
        buildtargetgroup = BuildTargetGroup.Android;
#endif
#if UNITY_IOS
        buildtargetgroup = BuildTargetGroup.iOS;
#endif
        string define = PlayerSettings.GetScriptingDefineSymbolsForGroup(buildtargetgroup);
        List<string> definelist = new List<string>(define.Split(';'));
        if (!definelist.Contains("Release"))
        {
            define += ";Release";
            PlayerSettings.SetScriptingDefineSymbolsForGroup(buildtargetgroup, define);
        }
    }
    [MenuItem("Lcc/Debug")]
    private static void Debug()
    {
        BuildTargetGroup buildtargetgroup;
#if UNITY_STANDALONE
        buildtargetgroup = BuildTargetGroup.Standalone;
#endif
#if UNITY_ANDROID
        buildtargetgroup = BuildTargetGroup.Android;
#endif
#if UNITY_IOS
        buildtargetgroup = BuildTargetGroup.iOS;
#endif
        string define = PlayerSettings.GetScriptingDefineSymbolsForGroup(buildtargetgroup);
        List<string> definelist = new List<string>(define.Split(';'));
        if (definelist.Contains("Release"))
        {
            definelist.Remove("Release");
            define = string.Empty;
            foreach (string item in definelist)
            {
                define += item + ";";
            }
            PlayerSettings.SetScriptingDefineSymbolsForGroup(buildtargetgroup, define);
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