using UnityEditor;
using UnityEngine;

public class UploadEditorWindow : EditorWindow
{
    private string _url;
    void OnGUI()
    {
        GUILayout.BeginHorizontal();
        GUILayout.Label("上传");
        _url = EditorGUILayout.TextField("url", _url);
        if (GUILayout.Button(new GUIContent("确定")))
        {
            
        }
        GUILayout.EndHorizontal();
    }
    [MenuItem("Lcc/UploadEditor")]
    private static void ShowLcc()
    {
        UploadEditorWindow upload = GetWindow<UploadEditorWindow>();
        upload.position = new Rect(0, 0, 600, 600);
        upload.Show();
    }
}