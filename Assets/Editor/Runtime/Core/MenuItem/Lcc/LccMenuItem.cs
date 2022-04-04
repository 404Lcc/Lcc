using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace LccEditor
{
    public class LccMenuItem
    {
        [MenuItem("Assets/工具箱/导表结构全部", false, 11)]
        public static void ExportScriptALL()
        {
            ExcelExportUtil.ExportScriptALL();
        }
        [MenuItem("Assets/工具箱/导表数据全部", false, 12)]
        public static void ExportDataALL()
        {
            ExcelExportUtil.ExportDataALL();
        }
        [MenuItem("Assets/工具箱/导表结构选中", false, 21)]
        public static void ExportScript()
        {
            List<string> pathList = new List<string>();
            foreach (Object item in Selection.objects)
            {
                string path = Application.dataPath.Replace("Assets", string.Empty) + AssetDatabase.GetAssetPath(item);
                pathList.Add(path);
            }
            ExcelExportUtil.ExportScriptSelect(pathList);
        }
        [MenuItem("Assets/工具箱/导表数据选中", false, 22)]
        public static void ExportData()
        {
            List<string> pathList = new List<string>();
            foreach (Object item in Selection.objects)
            {
                string path = Application.dataPath.Replace("Assets", string.Empty) + AssetDatabase.GetAssetPath(item);
                pathList.Add(path);
            }
            ExcelExportUtil.ExportDataSelect(pathList);
        }

        [MenuItem("Assets/工具箱/热更Panel", false, 31)]
        public static void CreateHotfixPanel()
        {
            string pathName = $"{CreateScriptUtil.GetSelectedPath()}/NewHotfixPanel.cs";
            Texture2D icon = (Texture2D)EditorGUIUtility.IconContent("cs Script Icon").image;
            ProjectWindowUtil.StartNameEditingIfProjectWindowExists(0, ScriptableObject.CreateInstance<CreateScriptUtil>(), pathName, icon, "Assets/Editor/Runtime/Core/Template/HotfixPanelTemplate.txt");
        }
        [MenuItem("Assets/工具箱/热更ViewModel", false, 32)]
        public static void CreateHotfixViewModel()
        {
            string pathName = $"{CreateScriptUtil.GetSelectedPath()}/NewHotfixViewModel.cs";
            Texture2D icon = (Texture2D)EditorGUIUtility.IconContent("cs Script Icon").image;
            ProjectWindowUtil.StartNameEditingIfProjectWindowExists(0, ScriptableObject.CreateInstance<CreateScriptUtil>(), pathName, icon, "Assets/Editor/Runtime/Core/Template/HotfixViewModelTemplate.txt");
        }
        [MenuItem("Assets/工具箱/主工程Panel", false, 33)]
        public static void CreateModelPanel()
        {
            string pathName = $"{CreateScriptUtil.GetSelectedPath()}/NewModelPanel.cs";
            Texture2D icon = (Texture2D)EditorGUIUtility.IconContent("cs Script Icon").image;
            ProjectWindowUtil.StartNameEditingIfProjectWindowExists(0, ScriptableObject.CreateInstance<CreateScriptUtil>(), pathName, icon, "Assets/Editor/Runtime/Core/Template/ModelPanelTemplate.txt");
        }
        [MenuItem("Assets/工具箱/主工程ViewModel", false, 34)]
        public static void CreateModelViewModel()
        {
            string pathName = $"{CreateScriptUtil.GetSelectedPath()}/NewModelViewModel.cs";
            Texture2D icon = (Texture2D)EditorGUIUtility.IconContent("cs Script Icon").image;
            ProjectWindowUtil.StartNameEditingIfProjectWindowExists(0, ScriptableObject.CreateInstance<CreateScriptUtil>(), pathName, icon, "Assets/Editor/Runtime/Core/Template/ModelViewModelTemplate.txt");
        }
    }
}