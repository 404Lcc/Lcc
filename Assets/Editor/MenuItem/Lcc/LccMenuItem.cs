using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace LccEditor
{
    public class LccMenuItem
    {
        [MenuItem("Assets/工具箱/热更Panel", false, 31)]
        public static void CreateHotfixPanel()
        {
            string pathName = $"{CreateScriptUtility.GetSelectedPath()}/NewHotfixPanel.cs";
            Texture2D icon = (Texture2D)EditorGUIUtility.IconContent("cs Script Icon").image;
            ProjectWindowUtil.StartNameEditingIfProjectWindowExists(0, ScriptableObject.CreateInstance<CreateScriptUtility>(), pathName, icon, "Assets/Editor/Utility/CreateScript/Template/HotfixPanelTemplate.txt");
        }
        [MenuItem("Assets/工具箱/热更ViewModel", false, 32)]
        public static void CreateHotfixViewModel()
        {
            string pathName = $"{CreateScriptUtility.GetSelectedPath()}/NewHotfixViewModel.cs";
            Texture2D icon = (Texture2D)EditorGUIUtility.IconContent("cs Script Icon").image;
            ProjectWindowUtil.StartNameEditingIfProjectWindowExists(0, ScriptableObject.CreateInstance<CreateScriptUtility>(), pathName, icon, "Assets/Editor/Utility/CreateScript/Template/HotfixViewModelTemplate.txt");
        }
        [MenuItem("Assets/工具箱/主工程Panel", false, 33)]
        public static void CreateModelPanel()
        {
            string pathName = $"{CreateScriptUtility.GetSelectedPath()}/NewModelPanel.cs";
            Texture2D icon = (Texture2D)EditorGUIUtility.IconContent("cs Script Icon").image;
            ProjectWindowUtil.StartNameEditingIfProjectWindowExists(0, ScriptableObject.CreateInstance<CreateScriptUtility>(), pathName, icon, "Assets/Editor/Utility/CreateScript/Template/ModelPanelTemplate.txt");
        }
        [MenuItem("Assets/工具箱/主工程ViewModel", false, 34)]
        public static void CreateModelViewModel()
        {
            string pathName = $"{CreateScriptUtility.GetSelectedPath()}/NewModelViewModel.cs";
            Texture2D icon = (Texture2D)EditorGUIUtility.IconContent("cs Script Icon").image;
            ProjectWindowUtil.StartNameEditingIfProjectWindowExists(0, ScriptableObject.CreateInstance<CreateScriptUtility>(), pathName, icon, "Assets/Editor/Utility/CreateScript/Template/ModelViewModelTemplate.txt");
        }
    }
}