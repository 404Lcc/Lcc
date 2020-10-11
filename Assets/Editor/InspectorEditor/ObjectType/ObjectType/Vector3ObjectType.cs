using System;
using UnityEditor;
using UnityEngine;

[ObjectType(typeof(Vector3))]
public class Vector3ObjectType : IObjectType
{
    public object Draw(Type type, string name, object value)
    {
        return EditorGUILayout.Vector3Field(name, (Vector3)value);
    }
}