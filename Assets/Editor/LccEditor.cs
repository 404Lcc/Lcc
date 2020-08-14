using UnityEditor;
using UnityEditorInternal;
using UnityEngine;

public class LccEditor : EditorWindow
{
    private string[] tags = { "GUI", "ModelManager", "HotfixManager", "AudioSource", "VideoPlayer" };
    private string[] layers = { };
    private string tag;
    private string layer;
    void OnGUI()
    {
        GUILayout.BeginHorizontal();
        GUILayout.Label("初始化Tag");
        if (GUILayout.Button(new GUIContent("确定")))
        {
            string succe = string.Empty;
            string failure = string.Empty;
            foreach (string item in tags)
            {
                if (AddTag(item))
                {
                    succe += item + " ";
                }
                else
                {
                    failure += item + " ";
                }
            }
            if (!string.IsNullOrEmpty(succe))
            {
                ShowNotification(new GUIContent("Tag初始化成功"));
            }
            else
            {
                ShowNotification(new GUIContent("Tag初始化失败"));
            }
        }
        GUILayout.EndHorizontal();

        GUILayout.BeginHorizontal();
        GUILayout.Label("初始化Layer");
        if (GUILayout.Button(new GUIContent("确定")))
        {
            string succe = string.Empty;
            string failure = string.Empty;
            foreach (string item in layers)
            {
                if (AddLayer(item))
                {
                    succe += item + " ";
                }
                else
                {
                    failure += item + " ";
                }
            }
            if (!string.IsNullOrEmpty(succe))
            {
                ShowNotification(new GUIContent("Layer初始化成功"));
            }
            else
            {
                ShowNotification(new GUIContent("Layer初始化失败"));
            }
        }
        GUILayout.EndHorizontal();

        tag = EditorGUILayout.TextField("自定义Tag", tag);
        if (GUILayout.Button(new GUIContent("增加Tag")))
        {
            if (string.IsNullOrEmpty(tag))
            {
                ShowNotification(new GUIContent("增加失败"));
                return;
            }
            string tips;
            if (AddTag(tag))
            {
                tips = tag + "增加成功";
            }
            else
            {
                tips = tag + "增加失败";
            }
            ShowNotification(new GUIContent(tips));
        }

        layer = EditorGUILayout.TextField("自定义Layer", layer);
        if (GUILayout.Button(new GUIContent("增加Layer")))
        {
            if (string.IsNullOrEmpty(layer))
            {
                ShowNotification(new GUIContent("增加失败"));
                return;
            }
            string tips;
            if (AddLayer(layer))
            {
                tips = layer + "增加成功";
            }
            else
            {
                tips = layer + "增加失败";
            }
            ShowNotification(new GUIContent(tips));
        }
    }
    [MenuItem("Lcc/LccEditor")]
    private static void ShowLcc()
    {
        LccEditor lcc = GetWindow<LccEditor>();
        lcc.position = new Rect(0, 0, 600, 600);
        lcc.Show();
    }
    /// <summary>
    /// tag是否存在
    /// </summary>
    /// <param name="tag"></param>
    /// <returns></returns>
    private bool TagExist(string tag)
    {
        foreach (string item in InternalEditorUtility.tags)
        {
            if (tag == item) return true;
        }
        return false;
    }
    /// <summary>
    /// layer是否存在
    /// </summary>
    /// <param name="layer"></param>
    /// <returns></returns>
    private bool LayerExist(string layer)
    {
        foreach (string item in InternalEditorUtility.layers)
        {
            if (layer == item) return true;
        }
        return false;
    }
    /// <summary>
    /// 添加Tag值
    /// </summary>
    /// <param name="tag"></param>
    /// <returns></returns>
    private bool AddTag(string tag)
    {
        if (!TagExist(tag))
        {
            SerializedObject tagmanager = new SerializedObject(AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/TagManager.asset")[0]);
            //获取tagmanager所有列表信息
            SerializedProperty serializedproperty = tagmanager.GetIterator();
            //判断向后是否还有信息,如果没有则返回false
            while (serializedproperty.NextVisible(true))
            {
                if (serializedproperty.name == "tags")
                {
                    for (int i = 0; i < serializedproperty.arraySize; i++)
                    {
                        //获取信息
                        SerializedProperty data = serializedproperty.GetArrayElementAtIndex(i);
                        //如果为空,则可以填写自己的层名称
                        if (string.IsNullOrEmpty(data.stringValue))
                        {
                            //设置名字
                            data.stringValue = tag;
                            //保存修改的属性
                            tagmanager.ApplyModifiedProperties();
                            return true;
                        }
                    }
                }
            }
            return false;
        }
        return false;
    }
    /// <summary>
    /// 加载Layer值
    /// </summary>
    /// <param name="layer"></param>
    /// <returns></returns>
    private bool AddLayer(string layer)
    {
        if (!LayerExist(layer))
        {
            SerializedObject tagmanager = new SerializedObject(AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/TagManager.asset")[0]);
            SerializedProperty serializedproperty = tagmanager.GetIterator();
            while (serializedproperty.NextVisible(true))
            {
                if (serializedproperty.name == "layers")
                {
                    //层默认是32个,只能从第8个开始写入自己的层
                    for (int i = 8; i < serializedproperty.arraySize; i++)
                    {
                        SerializedProperty data = serializedproperty.GetArrayElementAtIndex(i);
                        if (string.IsNullOrEmpty(data.stringValue))
                        {
                            data.stringValue = layer;
                            tagmanager.ApplyModifiedProperties();
                            return true;
                        }
                    }
                }
            }
            return false;
        }
        return false;
    }
}