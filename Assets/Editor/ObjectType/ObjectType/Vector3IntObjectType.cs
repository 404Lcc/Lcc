using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEngine;

[ObjectType(typeof(Vector3Int))]
public class Vector3IntObjectType : IObjectType
{
    public void Draw(object obj, FieldInfo field)
    {
        string name = field.Name.First().ToString().ToUpper() + field.Name.Substring(1);
        object value = field.GetValue(obj);
        field.SetValue(obj, EditorGUILayout.Vector3IntField(name, (Vector3Int)value));
    }
}