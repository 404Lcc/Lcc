using ILRuntime.Runtime.CLRBinding;
using ILRuntime.Runtime.Enviorment;
using ILRuntime.Runtime.Intepreter;
using LccModel;
using Sirenix.OdinInspector;
using System;
using System.IO;
using System.Reflection;
using UnityEditor;
using UnityEngine;
using AppDomain = ILRuntime.Runtime.Enviorment.AppDomain;
using FileUtil = LccModel.FileUtil;

namespace LccEditor
{
    public class ILRuntimeEditorWindowBase : AEditorWindowBase
    {
        [PropertySpace(10)]
        [HideLabel, DisplayAsString]
        public string info = "ILRuntime工具";
        [PropertySpace(10)]
        [LabelText("程序集名")]
        public string assemblieName = "Unity.Model";
        [PropertySpace(10)]
        [LabelText("命名空间")]
        public string namespaceName = "LccModel";
        [PropertySpace(10)]
        [LabelText("类名")]
        public string className;
        public ILRuntimeEditorWindowBase()
        {
        }
        public ILRuntimeEditorWindowBase(EditorWindow editorWindow) : base(editorWindow)
        {
        }
        [PropertySpace(10)]
        [LabelText("生成CLR绑定代码"), Button]
        public void BuildCLRBinding()
        {
            AppDomain appDomain = new AppDomain();
            using (MemoryStream dll = new MemoryStream(RijndaelUtil.RijndaelDecrypt("xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx", FileUtil.GetAsset("Assets/Bundles/DLL/Unity.Hotfix.dll.bytes"))))
            {
                appDomain.LoadAssembly(dll);
                ILRuntimeHelper.RegisterCrossBindingAdaptor(appDomain);
                BindingCodeGenerator.GenerateBindingCode(appDomain, "Assets/Framework/Runtime/Core/Manager/ILRuntime/Generated");
            }
            AssetDatabase.Refresh();
        }
        [PropertySpace(10)]
        [LabelText("生成适配器"), Button]
        public void BuildCrossBinding()
        {
            if (string.IsNullOrEmpty(assemblieName))
            {
                editorWindow.ShowNotification(new GUIContent("请输入程序集"));
                return;
            }
            if (string.IsNullOrEmpty(className))
            {
                editorWindow.ShowNotification(new GUIContent("请输入脚本名"));
                return;
            }

            Assembly assembly = null;
            foreach (var item in System.AppDomain.CurrentDomain.GetAssemblies())
            {
                if (item.GetName().Name == assemblieName)
                {
                    assembly = item;
                }
            }
            if (assembly == null)
            {
                editorWindow.ShowNotification(new GUIContent("没有这个程序集"));
                return;
            }
            Type type;
            if (string.IsNullOrEmpty(namespaceName))
            {
                type = assembly.GetType(className);
            }
            else
            {
                type = assembly.GetType($"{namespaceName}.{className}");
            }
            if (type == null)
            {
                editorWindow.ShowNotification(new GUIContent("没有此脚本"));
                return;
            }
            if (File.Exists($"Assets/Framework/Runtime/Core/Manager/ILRuntime/Adapter/{className}Adapter.cs"))
            {
                File.Delete($"Assets/Framework/Runtime/Core/Manager/ILRuntime/Adapter/{className}Adapter.cs");
            }
            FileUtil.SaveAsset($"Assets/Framework/Runtime/Core/Manager/ILRuntime/Adapter/{className}Adapter.cs", CrossBindingCodeGenerator.GenerateCrossBindingAdapterCode(type, "LccModel"));
            AssetDatabase.Refresh();
        }
    }
}