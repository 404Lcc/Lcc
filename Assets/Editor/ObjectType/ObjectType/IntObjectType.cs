using System;
using UnityEditor;

[ObjectType(typeof(int))]
public class IntObjectType : IObjectType
{
    public object Draw(Type type, string name, object value)
    {
        return EditorGUILayout.IntField(name, (int)value);
    }
}