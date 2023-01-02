using Sirenix.OdinInspector;
using UnityEditor;
using UnityEngine;

namespace LccEditor
{
    public class FrameworkEditorWindow : AEditorWindowBase
    {
        [PropertySpace(10)]
        [HideLabel, DisplayAsString]
        public string info = "Lcc是针对Unity开发的轻量级框架，可快速上手开发Steam、安卓、IOS等项目";
        public FrameworkEditorWindow()
        {
        }
        public FrameworkEditorWindow(EditorWindow editorWindow) : base(editorWindow)
        {
        }
        [PropertySpace(10)]
        [LabelText("Github"), Button(ButtonSizes.Gigantic)]
        public void OpenGithub()
        {
            Application.OpenURL("https://github.com/404Lcc/Lcc");
        }
    }
}