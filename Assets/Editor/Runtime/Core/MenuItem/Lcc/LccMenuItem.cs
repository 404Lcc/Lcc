using LccModel;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace LccEditor
{
    public class LccMenuItem
    {
        [MenuItem("Assets/Lcc/ExcelExport")]
        public static void ExcelExport()
        {
            ExcelExportUtil.ExportAll();
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