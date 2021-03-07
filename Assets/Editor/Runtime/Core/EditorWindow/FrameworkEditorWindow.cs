using UnityEditor;
using UnityEngine;

namespace LccEditor
{
    public class FrameworkEditorWindow : EditorWindow
    {
        [MenuItem("Lcc/Framework")]
        public static void ShowFramework()
        {
            FrameworkEditorWindow framework = GetWindow<FrameworkEditorWindow>();
            framework.position = new Rect(Screen.currentResolution.width / 2 - 500, Screen.currentResolution.height / 2 - 250, 1000, 500);
            framework.Show();
        }
    }
}