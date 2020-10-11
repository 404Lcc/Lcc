using System;
using UnityEditor;
using UnityEngine;

[ObjectType(typeof(Bounds))]
public class BoundsObjectType : IObjectType
{
    public object Draw(Type type, string name, object value)
    {
        return EditorGUILayout.BoundsField(name, (Bounds)value);
    }
}