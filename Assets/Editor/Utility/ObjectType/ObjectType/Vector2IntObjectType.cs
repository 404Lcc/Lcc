using System;
using UnityEditor;
using UnityEngine;

namespace LccEditor
{
    [ObjectType]
    public class Vector2IntObjectType : IObjectType
    {
        public bool IsType(Type type)
        {
            return type == typeof(Vector2Int);
        }
        public object Draw(Type memberType, string memberName, object value, object target)
        {
            return EditorGUILayout.Vector2IntField(memberName, (Vector2Int)value);
        }
    }
}