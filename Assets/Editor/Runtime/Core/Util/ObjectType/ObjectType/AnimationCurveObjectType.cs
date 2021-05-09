using System;
using UnityEditor;
using UnityEngine;

namespace LccEditor
{
    [ObjectType]
    public class AnimationCurveObjectType : IObjectType
    {
        public bool IsType(Type type)
        {
            return type == typeof(AnimationCurve);
        }
        public object Draw(Type memberType, string memberName, object value, object target)
        {
            return EditorGUILayout.CurveField(memberName, (AnimationCurve)value);
        }
    }
}