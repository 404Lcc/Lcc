using System;
using UnityEditor;
using UnityEngine;

[ObjectType(typeof(Rect))]
public class RectObjectType : IObjectType
{
    public object Draw(Type type, string name, object value)
    {
        return EditorGUILayout.RectField(name, (Rect)value);
    }
}