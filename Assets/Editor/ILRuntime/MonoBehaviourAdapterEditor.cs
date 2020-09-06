using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(MonoBehaviourAdapter.Adaptor), true)]
public class MonoBehaviourAdapterEditor : UnityEditor.UI.GraphicEditor
{
    public override void OnInspectorGUI()
    {
        serializedObject.Update();
        MonoBehaviourAdapter.Adaptor clr = target as MonoBehaviourAdapter.Adaptor;
        var instance = clr.ILInstance;
        if (instance != null)
        {
            EditorGUILayout.LabelField("Script", clr.ILInstance.Type.FullName);

            int index = 0;
            foreach (var i in instance.Type.FieldMapping)
            {
                //这里是取的所有字段，没有处理不是public的
                var name = i.Key;
                //在这里不能用i.Value，因为Unity有HideInInspector方法，隐藏序列化的值，但是还是会被计数
                var type = instance.Type.FieldTypes[index];
                index++;

                var cType = type.TypeForCLR;
                //如果是基础类型
                if (cType.IsPrimitive)
                {
                    if (cType == typeof(float))
                    {
                        instance[i.Value] = EditorGUILayout.FloatField(name, (float)instance[i.Value]);
                    }
                    else
                        //剩下的大家自己补吧
                        throw new System.NotImplementedException();
                }
                else
                {
                    object obj = instance[i.Value];
                    if (typeof(Object).IsAssignableFrom(cType))
                    {
                        //处理Unity类型
                        var res = EditorGUILayout.ObjectField(name, obj as Object, cType, true);
                        instance[i.Value] = res;
                    }
                    else
                    {
                        //其他类型现在没法处理
                        if (obj != null)
                            EditorGUILayout.LabelField(name, obj.ToString());
                        else
                            EditorGUILayout.LabelField(name, "(null)");
                    }
                }
            }
        }
    }
}