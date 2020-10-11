using System;
using UnityEditor;

[ObjectType(typeof(bool))]
public class BoolObjectType : IObjectType
{
    public object Draw(Type type, string name, object value)
    {
        return EditorGUILayout.Toggle(name, (bool)value);
    }
}