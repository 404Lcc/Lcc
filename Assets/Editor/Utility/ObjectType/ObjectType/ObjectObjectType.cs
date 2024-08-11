using System;
using UnityEditor;

namespace LccEditor
{
    public class ObjectObjectType
    {
        public bool isFoldout;
        public void Draw(Type memberType, string memberName, object value, object target, int indentLevel)
        {
            isFoldout = EditorGUILayout.Foldout(isFoldout, memberName, true);
            if (isFoldout)
            {
                EditorGUI.indentLevel = indentLevel;
                ObjectTypeUtility.Draw(value, indentLevel + 1);
                EditorGUI.indentLevel = indentLevel;
            }
        }
    }
}