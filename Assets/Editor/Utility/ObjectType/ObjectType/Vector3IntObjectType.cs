using System;
using UnityEditor;
using UnityEngine;

namespace LccEditor
{
    [ObjectType]
    public class Vector3IntObjectType : IObjectType
    {
        public bool IsType(Type type)
        {
            return type == typeof(Vector3Int);
        }
        public object Draw(Type memberType, string memberName, object value, object target)
        {
            return EditorGUILayout.Vector3IntField(memberName, (Vector3Int)value);
        }
    }
}