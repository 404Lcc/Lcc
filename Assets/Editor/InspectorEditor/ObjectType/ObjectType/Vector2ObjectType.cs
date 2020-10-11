using System;
using UnityEditor;
using UnityEngine;

[ObjectType(typeof(Vector2))]
public class Vector2ObjectType : IObjectType
{
    public object Draw(Type type, string name, object value)
    {
        return EditorGUILayout.Vector2Field(name, (Vector2)value);
    }
}