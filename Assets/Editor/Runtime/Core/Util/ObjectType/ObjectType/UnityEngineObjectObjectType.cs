using System;
using UnityEditor;
using Object = UnityEngine.Object;

namespace LccEditor
{
    [ObjectType]
    public class UnityEngineObjectObjectType : IObjectType
    {
        public bool IsType(Type type)
        {
            return type == typeof(Object) || type.IsSubclassOf(typeof(Object));
        }
        public object Draw(Type memberType, string memberName, object value, object target)
        {
            return EditorGUILayout.ObjectField(memberName, (Object)value, memberType, true);
        }
    }
}