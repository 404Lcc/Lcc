using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEngine;

[ObjectType(typeof(Vector2Int))]
public class Vector2IntObjectType : IObjectType
{
    public void Draw(object obj, FieldInfo field)
    {
        string name = field.Name.First().ToString().ToUpper() + field.Name.Substring(1);
        object value = field.GetValue(obj);
        field.SetValue(obj, EditorGUILayout.Vector2IntField(name, (Vector2Int)value));
    }
}