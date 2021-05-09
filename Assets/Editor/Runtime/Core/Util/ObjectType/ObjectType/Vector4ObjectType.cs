using System;
using UnityEditor;
using UnityEngine;

namespace LccEditor
{
    [ObjectType]
    public class Vector4ObjectType : IObjectType
    {
        public bool IsType(Type type)
        {
            return type == typeof(Vector4);
        }
        public object Draw(Type memberType, string memberName, object value, object target)
        {
            return EditorGUILayout.Vector4Field(memberName, (Vector4)value);
        }
    }
}