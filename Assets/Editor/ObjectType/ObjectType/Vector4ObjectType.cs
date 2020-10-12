using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEngine;

[ObjectType(typeof(Vector4))]
public class Vector4ObjectType : IObjectType
{
    public void Draw(object obj, FieldInfo field)
    {
        string name = field.Name.First().ToString().ToUpper() + field.Name.Substring(1);
        object value = field.GetValue(obj);
        field.SetValue(obj, EditorGUILayout.Vector4Field(name, (Vector4)value));
    }
}