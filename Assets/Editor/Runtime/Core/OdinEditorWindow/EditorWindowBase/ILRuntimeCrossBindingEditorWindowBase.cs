using ILRuntime.Runtime.Enviorment;
using Sirenix.OdinInspector;
using System;
using System.IO;
using System.Reflection;
using UnityEditor;
using UnityEngine;
using FileUtil = LccModel.FileUtil;

namespace LccEditor
{
    public class ILRuntimeCrossBindingEditorWindowBase : AEditorWindowBase
    {
        [PropertySpace(10)]
        [HideLabel, DisplayAsString]
        public string info = "ILRuntime适配器生成工具";
        [PropertySpace(10)]
        [LabelText("程序集")]
        public string assets = "Unity.Model";
        [PropertySpace(10)]
        [LabelText("命名空间")]
        public string namespaceName = "LccModel";
        [PropertySpace(10)]
        [LabelText("类名")]
        public string className;
        public ILRuntimeCrossBindingEditorWindowBase()
        {
        }
        public ILRuntimeCrossBindingEditorWindowBase(EditorWindow editorWindow) : base(editorWindow)
        {
        }
        [PropertySpace(10)]
        [LabelText("生成适配器"), Button]
        public void BuildILRuntimeCrossBinding()
        {
            if (string.IsNullOrEmpty(assets))
            {
                editorWindow.ShowNotification(new GUIContent("请输入程序集"));
                return;
            }
            if (string.IsNullOrEmpty(className))
            {
                editorWindow.ShowNotification(new GUIContent("请输入脚本名"));
                return;
            }
            string path = $"Library/ScriptAssemblies/{assets}.dll";
            if (!File.Exists(path))
            {
                editorWindow.ShowNotification(new GUIContent("程序集路径错误"));
                return;
            }
            Type type;
            if (string.IsNullOrEmpty(namespaceName))
            {
                type = Assembly.LoadFile(path).GetType(className);
            }
            else
            {
                type = Assembly.LoadFile(path).GetType($"{namespaceName}.{className}");
            }
            if (type == null)
            {
                editorWindow.ShowNotification(new GUIContent("没有此脚本"));
                return;
            }
            if (File.Exists($"Assets/Scripts/Runtime/Core/Manager/ILRuntime/Adapter/{className}Adapter.cs"))
            {
                File.Delete($"Assets/Scripts/Runtime/Core/Manager/ILRuntime/Adapter/{className}Adapter.cs");
            }
            FileUtil.SaveAsset($"Assets/Scripts/Runtime/Core/Manager/ILRuntime/Adapter/{className}Adapter.cs", CrossBindingCodeGenerator.GenerateCrossBindingAdapterCode(type, "LccModel"));
            AssetDatabase.Refresh();
        }
    }
}