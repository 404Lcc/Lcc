using System;
using UnityEditor;
using UnityEngine;

namespace LccEditor
{
    [ObjectType]
    public class BoundsObjectType : IObjectType
    {
        public bool IsType(Type type)
        {
            return type == typeof(Bounds);
        }
        public object Draw(Type memberType, string memberName, object value, object target)
        {
            return EditorGUILayout.BoundsField(memberName, (Bounds)value);
        }
    }
}