using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace LccEditor
{
    [ObjectType(typeof(object))]
    public class ObjectObjectType : IObjectType
    {
        public bool isFoldout;
        public void Draw(object obj, FieldInfo field)
        {
            if (obj == null) return;
            string name = field.Name.First().ToString().ToUpper() + field.Name.Substring(1);
            isFoldout = EditorGUILayout.Foldout(isFoldout, name, true);
            if (isFoldout)
            {
                FieldInfo[] fields = obj.GetType().GetFields(BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance | BindingFlags.FlattenHierarchy);
                foreach (FieldInfo item in fields)
                {
                    if (item.FieldType.IsDefined(typeof(HideInInspector), false))
                    {
                        continue;
                    }
                    if (item.IsDefined(typeof(HideInInspector), false))
                    {
                        continue;
                    }
                    if (ObjectTypeUtil.objectTypes.ContainsKey(item.FieldType))
                    {
                        ((IObjectType)ObjectTypeUtil.objectTypes[item.FieldType]).Draw(obj, item);
                        continue;
                    }
                }
            }
        }
    }
}