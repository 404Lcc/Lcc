using LccModel;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace LccEditor
{
    [CustomEditor(typeof(LccView))]
    public class LccViewEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();
            LccView lccView = (LccView)target;
            if (GUILayout.Button(lccView.className))
            {
                Selection.activeObject = null;
                EditorGUIUtility.PingObject(Selection.activeObject);
                string directoryName = string.Empty;
                switch (Path.GetFileNameWithoutExtension(lccView.type.GetType().Assembly.ManifestModule.Name))
                {
                    case "Unity.Model":
                        directoryName = "Scripts";
                        break;
                    case "Unity.Hotfix":
                        directoryName = "Hotfix";
                        break;
                    case "ILRuntime":
                        directoryName = "Hotfix";
                        break;
                }
                string fileName = lccView.className.Split('.')[1];
                string[] filePaths = Directory.GetFiles($"Assets/{directoryName}", "*.cs", SearchOption.AllDirectories);
                foreach (string item in filePaths)
                {
                    if (item.Substring(item.LastIndexOf(@"\") + 1) == $"{fileName}.cs")
                    {
                        Selection.activeObject = AssetDatabase.LoadAssetAtPath<TextAsset>(item);
                        EditorGUIUtility.PingObject(Selection.activeObject);
                    }
                }
            }
            ObjectTypeUtil.Draw(lccView.type);
        }
    }
}