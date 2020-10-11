using System;
using UnityEditor;
using UnityEngine;

[ObjectType(typeof(Vector2Int))]
public class Vector2IntObjectType : IObjectType
{
    public object Draw(Type type, string name, object value)
    {
        return EditorGUILayout.Vector2IntField(name, (Vector2Int)value);
    }
}