using System;
using UnityEditor;

[ObjectType(typeof(double))]
public class DoubleObjectType : IObjectType
{
    public object Draw(Type type, string name, object value)
    {
        return EditorGUILayout.DoubleField(name, (double)value);
    }
}