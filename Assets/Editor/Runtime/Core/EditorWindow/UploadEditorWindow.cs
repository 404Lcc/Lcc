using UnityEditor;
using UnityEngine;

namespace LccEditor
{
    public class UploadEditorWindow : EditorWindow
    {
        public string url;
        void OnGUI()
        {
            GUILayout.BeginHorizontal();
            GUILayout.Label("上传");
            url = EditorGUILayout.TextField("url", url);
            if (GUILayout.Button(new GUIContent("确定")))
            {

            }
            GUILayout.EndHorizontal();
        }
        [MenuItem("Lcc/Upload")]
        public static void ShowUpload()
        {
            UploadEditorWindow upload = GetWindow<UploadEditorWindow>();
            upload.position = new Rect(Screen.currentResolution.width / 2 - 500, Screen.currentResolution.height / 2 - 250, 1000, 500);
            upload.Show();
        }
    }
}