using System;
using UnityEditor;

[ObjectType(typeof(char))]
public class CharObjectType : IObjectType
{
    public object Draw(Type type, string name, object value)
    {
        string info = EditorGUILayout.TextField(name, ((char)value).ToString());
        return info.Length > 0 ? info[0] : default;
    }
}