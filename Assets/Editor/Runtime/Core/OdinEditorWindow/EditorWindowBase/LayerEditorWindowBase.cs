using Sirenix.OdinInspector;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;

namespace LccEditor
{
    public class LayerEditorWindowBase : AEditorWindowBase
    {
        [PropertySpace(10)]
        [HideLabel, DisplayAsString]
        public string info = "层工具";
        [PropertySpace(10)]
        [LabelText("层名")]
        public string layerName;
        public LayerEditorWindowBase()
        {
        }
        public LayerEditorWindowBase(EditorWindow editorWindow) : base(editorWindow)
        {
        }
        [PropertySpace(10)]
        [LabelText("增加层"), Button]
        public void AddLayer()
        {
            if (string.IsNullOrEmpty(layerName))
            {
                editorWindow.ShowNotification(new GUIContent("请输入层名"));
                return;
            }
            bool success = AddLayer(layerName);
            if (success)
            {
                editorWindow.ShowNotification(new GUIContent("Layer增加成功"));
            }
            else
            {
                editorWindow.ShowNotification(new GUIContent("Layer增加失败"));
            }
        }
        [PropertySpace(10)]
        [LabelText("移除层"), Button]
        public void RemoveLayer()
        {
            if (string.IsNullOrEmpty(layerName))
            {
                editorWindow.ShowNotification(new GUIContent("请输入层名"));
                return;
            }
            bool success = RemoveLayer(layerName);
            if (success)
            {
                editorWindow.ShowNotification(new GUIContent("Layer移除成功"));
            }
            else
            {
                editorWindow.ShowNotification(new GUIContent("Layer移除失败"));
            }
        }
        public bool LayerExist(string layer)
        {
            foreach (string item in InternalEditorUtility.layers)
            {
                if (layer == item) return true;
            }
            return false;
        }
        public bool AddLayer(string layer)
        {
            if (!LayerExist(layer))
            {
                SerializedObject tagManager = new SerializedObject(AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/TagManager.asset")[0]);
                SerializedProperty serializedProperty = tagManager.GetIterator();
                while (serializedProperty.NextVisible(true))
                {
                    if (serializedProperty.name == "layers")
                    {
                        //层默认是32个,只能从第8个开始写入自己的层
                        for (int i = 8; i < serializedProperty.arraySize; i++)
                        {
                            SerializedProperty data = serializedProperty.GetArrayElementAtIndex(i);
                            if (string.IsNullOrEmpty(data.stringValue))
                            {
                                data.stringValue = layer;
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
        public bool RemoveLayer(string layer)
        {
            if (!LayerExist(layer))
            {
                SerializedObject tagManager = new SerializedObject(AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/TagManager.asset")[0]);
                SerializedProperty serializedProperty = tagManager.GetIterator();
                while (serializedProperty.NextVisible(true))
                {
                    if (serializedProperty.name == "layers")
                    {
                        //层默认是32个,只能从第8个开始写入自己的层
                        for (int i = 8; i < serializedProperty.arraySize; i++)
                        {
                            SerializedProperty data = serializedProperty.GetArrayElementAtIndex(i);
                            if (data.stringValue == layer)
                            {
                                data.stringValue = string.Empty;
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