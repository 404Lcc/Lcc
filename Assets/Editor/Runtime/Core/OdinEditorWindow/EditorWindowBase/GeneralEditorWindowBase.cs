using LccModel;
using Sirenix.OdinInspector;
using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace LccEditor
{
    public class GeneralEditorWindowBase : AEditorWindowBase
    {
        [PropertySpace(10)]
        [HideLabel, DisplayAsString]
        public string info = "通用功能";
        public GeneralEditorWindowBase()
        {
        }
        public GeneralEditorWindowBase(EditorWindow editorWindow) : base(editorWindow)
        {
        }
        [PropertySpace(10)]
        [LabelText("打开PersistentData路径"), Button]
        public void OpenPersistentDataPath()
        {
            EditorUtility.OpenWithDefaultApp(Application.persistentDataPath);
        }
        [PropertySpace(10)]
        [LabelText("生成PC/Android/IOS文件"), Button]
        public void BuildPlayer()
        {
            List<string> argList = new List<string>();
            foreach (string item in Environment.GetCommandLineArgs())
            {
                argList.Add(item);
            }
            string name = $"{PlayerSettings.productName} v{PlayerSettings.bundleVersion}";
#if UNITY_STANDALONE
            name = $"{name}.exe";
#endif
#if UNITY_ANDROID
            name = $"{name}.apk";
#endif
            string locationPathName = $"{PathUtil.GetPersistentDataPath("Build")}/{name}";
            BuildPipeline.BuildPlayer(EditorBuildSettings.scenes, locationPathName, EditorUserBuildSettings.activeBuildTarget, EditorUserBuildSettings.development ? BuildOptions.Development : BuildOptions.None);
        }
    }
}