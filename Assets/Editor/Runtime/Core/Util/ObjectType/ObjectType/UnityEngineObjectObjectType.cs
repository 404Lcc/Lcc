using System.Linq;
using System.Reflection;
using UnityEditor;
using Object = UnityEngine.Object;

namespace LccEditor
{
    [ObjectType(typeof(Object))]
    public class UnityEngineObjectObjectType : IObjectType
    {
        public void Draw(object obj, FieldInfo field)
        {
            string name = field.Name.First().ToString().ToUpper() + field.Name.Substring(1);
            object value = field.GetValue(obj);
            field.SetValue(obj, EditorGUILayout.ObjectField(name, (Object)value, field.FieldType, true));
        }
    }
}