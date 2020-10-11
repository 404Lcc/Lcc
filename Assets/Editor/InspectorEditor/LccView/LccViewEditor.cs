using Model;
using System.IO;
using UnityEditor;
using UnityEngine;

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
            string[] infos = lccView.className.Split('.');
            string name = infos[0].Replace(lccView.GetType().Namespace, "Scripts");
            string[] files = Directory.GetFiles("Assets/" + name, "*.cs", SearchOption.AllDirectories);
            foreach (string item in files)
            {
                if (item.Substring(item.LastIndexOf(@"\") + 1) == infos[1] + ".cs")
                {
                    Selection.activeObject = AssetDatabase.LoadAssetAtPath<TextAsset>(item);
                    EditorGUIUtility.PingObject(Selection.activeObject);
                }
            }
        }
        ObjectTypeUtil.Draw(lccView.type);
    }
}