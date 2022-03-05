using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;

namespace LccEditor
{
    public class ListObjectType
    {
        public static Dictionary<(object, object), ObjectObjectType> objectObjectTypeDict = new Dictionary<(object, object), ObjectObjectType>();
        public bool isFoldout;
        public void Draw(Type memberType, string memberName, object value, object target, int indentLevel)
        {
            isFoldout = EditorGUILayout.Foldout(isFoldout, memberName, true);
            if (isFoldout)
            {
                EditorGUI.indentLevel = indentLevel + 1;
                IList iList = (IList)value;
                EditorGUILayout.LabelField("Size", iList.Count.ToString());
                for (int i = 0; i < iList.Count; i++)
                {
                    if (iList[i] == null) continue;
                    if (objectObjectTypeDict.ContainsKey((value, iList[i])))
                    {
                        ObjectObjectType objectObjectType = objectObjectTypeDict[(value, iList[i])];
                        objectObjectType.Draw(iList[i].GetType(), $"Element {i}", iList[i], null, indentLevel + 1);
                        continue;
                    }
                    if (iList[i].GetType().Assembly.ManifestModule.Name == "ILRuntime.dll" || iList[i].GetType().Assembly.ManifestModule.Name == "Unity.Model.dll")
                    {
                        ObjectObjectType objectObjectType = new ObjectObjectType();
                        objectObjectType.Draw(iList[i].GetType(), $"Element {i}", iList[i], null, indentLevel + 1);
                        objectObjectTypeDict.Add((value, iList[i]), objectObjectType);
                        continue;
                    }
                    foreach (IObjectType objectTypeItem in ObjectTypeUtil.objectList)
                    {
                        if (!objectTypeItem.IsType(iList[i].GetType()))
                        {
                            continue;
                        }
                        iList[i] = objectTypeItem.Draw(iList[i].GetType(), $"Element {i}", iList[i], null);
                    }
                }
                EditorGUI.indentLevel = indentLevel;
            }
        }
    }
}