using System;
using UnityEditor;

[ObjectType(typeof(string))]
public class StringObjectType : IObjectType
{
    public object Draw(Type type, string name, object value)
    {
        return EditorGUILayout.TextField(name, (string)value);
    }
}