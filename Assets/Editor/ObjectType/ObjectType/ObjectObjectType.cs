using System;
using UnityEditor;
using Object = UnityEngine.Object;

[ObjectType(typeof(Object))]
public class ObjectObjectType : IObjectType
{
    public object Draw(Type type, string name, object value)
    {
        return EditorGUILayout.ObjectField(name, (Object)value, type, true);
    }
}