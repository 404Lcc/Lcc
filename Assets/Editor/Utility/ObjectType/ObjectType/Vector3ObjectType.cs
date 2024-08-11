using System;
using UnityEditor;
using UnityEngine;

namespace LccEditor
{
    [ObjectType]
    public class Vector3ObjectType : IObjectType
    {
        public bool IsType(Type type)
        {
            return type == typeof(Vector3);
        }
        public object Draw(Type memberType, string memberName, object value, object target)
        {
            return EditorGUILayout.Vector3Field(memberName, (Vector3)value);
        }
    }
}