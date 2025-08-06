using LccModel;
using Sirenix.OdinInspector;
using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace LccEditor
{
    [MenuTree("通用功能", 2)]
    public class GeneralEditorWindow : AEditorWindowBase
    {
        [PropertySpace(10)]
        [HideLabel, DisplayAsString]
        public string info = "通用功能";
        public GeneralEditorWindow()
        {
        }
        public GeneralEditorWindow(EditorWindow editorWindow) : base(editorWindow)
        {
        }
        [PropertySpace(10)]
        [LabelText("打开PersistentData路径"), Button(ButtonSizes.Gigantic)]
        public void OpenPersistentDataPath()
        {
            EditorUtility.OpenWithDefaultApp(Application.persistentDataPath);
        }
    }
}