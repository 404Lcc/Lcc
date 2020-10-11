using System;
using UnityEditor;
using UnityEngine;

[ObjectType(typeof(Vector3Int))]
public class Vector3IntObjectType : IObjectType
{
    public object Draw(Type type, string name, object value)
    {
        return EditorGUILayout.Vector3IntField(name, (Vector3Int)value);
    }
}