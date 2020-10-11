using System;
using UnityEditor;
using UnityEngine;

[ObjectType(typeof(Color))]
public class ColorObjectType : IObjectType
{
    public object Draw(Type type, string name, object value)
    {
        return EditorGUILayout.ColorField(name, (Color)value);
    }
}