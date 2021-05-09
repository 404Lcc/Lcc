using System;
using UnityEditor;
using UnityEngine;

namespace LccEditor
{
    [ObjectType]
    public class ColorObjectType : IObjectType
    {
        public bool IsType(Type type)
        {
            return type == typeof(Color);
        }
        public object Draw(Type memberType, string memberName, object value, object target)
        {
            return EditorGUILayout.ColorField(memberName, (Color)value);
        }
    }
}