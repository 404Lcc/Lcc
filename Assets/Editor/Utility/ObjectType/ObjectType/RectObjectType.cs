using System;
using UnityEditor;
using UnityEngine;

namespace LccEditor
{
    [ObjectType]
    public class RectObjectType : IObjectType
    {
        public bool IsType(Type type)
        {
            return type == typeof(Rect);
        }
        public object Draw(Type memberType, string memberName, object value, object target)
        {
            return EditorGUILayout.RectField(memberName, (Rect)value);
        }
    }
}