using System;
using UnityEditor;

[ObjectType(typeof(long))]
public class LongObjectType : IObjectType
{
    public object Draw(Type type, string name, object value)
    {
        return EditorGUILayout.LongField(name, (long)value);
    }
}