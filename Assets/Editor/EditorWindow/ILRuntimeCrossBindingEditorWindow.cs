using ILRuntime.Runtime.Enviorment;
using System;
using System.IO;
using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace LccEditor
{
    public class ILRuntimeCrossBindingEditorWindow : EditorWindow
    {
        private string _namespace = "Model";
        private string _class;
        void OnGUI()
        {
            GUILayout.BeginHorizontal();
            GUILayout.Label("生成适配器");
            GUILayout.EndHorizontal();
            GUILayout.BeginHorizontal();
            _namespace = EditorGUILayout.TextField("命名空间", _namespace);
            _class = EditorGUILayout.TextField("类名", _class);
            GUILayout.EndHorizontal();
            GUILayout.BeginHorizontal();
            if (GUILayout.Button(new GUIContent("生成")))
            {
                string path = "Library/ScriptAssemblies/Unity.Model.dll";
                Type type = Assembly.LoadFile(path).GetType(_namespace + "." + _class);
                if (type == null)
                {
                    ShowNotification(new GUIContent("没有此脚本"));
                    return;
                }
                if (File.Exists("Assets/Scripts/Runtime/Core/Manager/ILRuntime/Adapter/" + _class + "Adapter.cs"))
                {
                    File.Delete("Assets/Scripts/Runtime/Core/Manager/ILRuntime/Adapter/" + _class + "Adapter.cs");
                }
                using (StreamWriter sw = new StreamWriter("Assets/Scripts/Runtime/Core/Manager/ILRuntime/Adapter/" + _class + "Adapter.cs"))
                {
                    sw.WriteLine(CrossBindingCodeGenerator.GenerateCrossBindingAdapterCode(type, _namespace));
                }
                AssetDatabase.Refresh();
            }
            GUILayout.EndHorizontal();
        }
        [MenuItem("Lcc/ILRuntimeCrossBindingEditor")]
        private static void ShowILRuntimeCrossBinding()
        {
            ILRuntimeCrossBindingEditorWindow ilRuntimeCrossBinding = GetWindow<ILRuntimeCrossBindingEditorWindow>();
            ilRuntimeCrossBinding.position = new Rect(0, 0, 600, 600);
            ilRuntimeCrossBinding.Show();
        }
    }
}