using System;
using UnityEditor;
using UnityEngine;

namespace LccEditor
{
    [ObjectType]
    public class Vector2ObjectType : IObjectType
    {
        public bool IsType(Type type)
        {
            return type == typeof(Vector2);
        }
        public object Draw(Type memberType, string memberName, object value, object target)
        {
            return EditorGUILayout.Vector2Field(memberName, (Vector2)value);
        }
    }
}