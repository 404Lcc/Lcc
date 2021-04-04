using Sirenix.OdinInspector;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;

namespace LccEditor
{
    public class TagEditorWindowBase : AEditorWindowBase
    {
        [PropertySpace(10)]
        [HideLabel, DisplayAsString]
        public string info = "标签工具";
        [PropertySpace(10)]
        [LabelText("标签名")]
        public string tagName;
        public TagEditorWindowBase()
        {
        }
        public TagEditorWindowBase(EditorWindow editorWindow) : base(editorWindow)
        {
        }
        [PropertySpace(10)]
        [LabelText("增加标签"), Button]
        public void AddTag()
        {
            if (string.IsNullOrEmpty(tagName))
            {
                editorWindow.ShowNotification(new GUIContent("请输入标签名"));
                return;
            }
            bool success = AddTag(tagName);
            if (success)
            {
                editorWindow.ShowNotification(new GUIContent("Tag增加成功"));
            }
            else
            {
                editorWindow.ShowNotification(new GUIContent("Tag增加失败"));
            }
        }
        [PropertySpace(10)]
        [LabelText("移除标签"), Button]
        public void RemoveTag()
        {
            if (string.IsNullOrEmpty(tagName))
            {
                editorWindow.ShowNotification(new GUIContent("请输入标签名"));
                return;
            }
            bool success = RemoveTag(tagName);
            if (success)
            {
                editorWindow.ShowNotification(new GUIContent("Tag移除成功"));
            }
            else
            {
                editorWindow.ShowNotification(new GUIContent("Tag移除失败"));
            }
        }
        public bool TagExist(string tag)
        {
            foreach (string item in InternalEditorUtility.tags)
            {
                if (tag == item) return true;
            }
            return false;
        }
        public bool AddTag(string tag)
        {
            if (!TagExist(tag))
            {
                SerializedObject tagManager = new SerializedObject(AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/TagManager.asset")[0]);
                //获取tagmanager所有列表信息
                SerializedProperty serializedProperty = tagManager.GetIterator();
                //判断向后是否还有信息,如果没有则返回false
                while (serializedProperty.NextVisible(true))
                {
                    if (serializedProperty.name == "tags")
                    {
                        for (int i = 0; i < serializedProperty.arraySize; i++)
                        {
                            //获取信息
                            SerializedProperty data = serializedProperty.GetArrayElementAtIndex(i);
                            //如果为空,则可以填写自己的层名称
                            if (string.IsNullOrEmpty(data.stringValue))
                            {
                                //设置名字
                                data.stringValue = tag;
                                //保存修改的属性
                                tagManager.ApplyModifiedProperties();
                                return true;
                            }
                        }
                    }
                }
                return false;
            }
            return false;
        }
        public bool RemoveTag(string tag)
        {
            if (TagExist(tag))
            {
                SerializedObject tagManager = new SerializedObject(AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/TagManager.asset")[0]);
                //获取tagmanager所有列表信息
                SerializedProperty serializedProperty = tagManager.GetIterator();
                //判断向后是否还有信息,如果没有则返回false
                while (serializedProperty.NextVisible(true))
                {
                    if (serializedProperty.name == "tags")
                    {
                        for (int i = 0; i < serializedProperty.arraySize; i++)
                        {
                            //获取信息
                            SerializedProperty data = serializedProperty.GetArrayElementAtIndex(i);
                            if (data.stringValue == tag)
                            {
                                data.stringValue = string.Empty;
                                //保存修改的属性
                                tagManager.ApplyModifiedProperties();
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
}