using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace LccEditor
{
    [ObjectType(typeof(AnimationCurve))]
    public class AnimationCurveObjectType : IObjectType
    {
        public void Draw(object obj, FieldInfo field)
        {
            string name = field.Name.First().ToString().ToUpper() + field.Name.Substring(1);
            object value = field.GetValue(obj);
            field.SetValue(obj, EditorGUILayout.CurveField(name, (AnimationCurve)value));
        }
    }
}