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
        [MenuItem("Lcc/UploadEditor")]
        public static void ShowUpload()
        {
            UploadEditorWindow upload = GetWindow<UploadEditorWindow>();
            upload.position = new Rect(0, 0, 600, 600);
            upload.Show();
        }
    }
}