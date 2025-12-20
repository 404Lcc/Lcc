using System.IO;
using LccHotfix;
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
                string directoryName = string.Empty;
                switch (Path.GetFileNameWithoutExtension(lccView.type.GetType().Assembly.ManifestModule.Name))
                {
                    case "Unity.Model":
                        directoryName = "Launcher";
                        break;
                    case "Unity.Hotfix":
                        directoryName = "Hotfix";
                        break;
                }
                string fileName = lccView.className;
                string[] filePaths = Directory.GetFiles($"Assets/{directoryName}", "*.cs", SearchOption.AllDirectories);
                foreach (string item in filePaths)
                {
                    if (item.Substring(item.LastIndexOf(@"\") + 1) == $"{fileName}.cs")
                    {
                        EditorGUIUtility.PingObject(AssetDatabase.LoadAssetAtPath<TextAsset>(item));
                        AssetDatabase.OpenAsset(AssetDatabase.LoadAssetAtPath<Object>(item), 0);
                    }
                }
            }
            ObjectTypeUtility.Draw(lccView.type, 0);
        }
    }
}