using System;
using UnityEditor;
using UnityEngine;

[ObjectType(typeof(Vector4))]
public class Vector4ObjectType : IObjectType
{
    public object Draw(Type type, string name, object value)
    {
        return EditorGUILayout.Vector4Field(name, (Vector4)value);
    }
}