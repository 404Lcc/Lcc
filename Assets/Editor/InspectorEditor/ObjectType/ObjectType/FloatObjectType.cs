using System;
using UnityEditor;

[ObjectType(typeof(float))]
public class FloatObjectType : IObjectType
{
    public object Draw(Type type, string name, object value)
    {
        return EditorGUILayout.FloatField(name, (float)value);
    }
}