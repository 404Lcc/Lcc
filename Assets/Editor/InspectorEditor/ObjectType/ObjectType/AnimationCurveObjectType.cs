using System;
using UnityEditor;
using UnityEngine;

[ObjectType(typeof(AnimationCurve))]
public class AnimationCurveObjectType : IObjectType
{
    public object Draw(Type type, string name, object value)
    {
        return EditorGUILayout.CurveField(name, (AnimationCurve)value);
    }
}