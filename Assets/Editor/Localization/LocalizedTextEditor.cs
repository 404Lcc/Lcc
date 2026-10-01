using LccHotfix;
using UnityEditor;
using UnityEngine;

namespace LccEditor
{
    [CustomEditor(typeof(LocalizedText), true)]
    [CanEditMultipleObjects]
    public class LocalizedTextEditor : Editor
    {
        private SerializedProperty _key;

        private void OnEnable()
        {
            _key = serializedObject.FindProperty("key");
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            EditorGUILayout.PropertyField(_key);
            EditorGUILayout.Space();

            if (GUILayout.Button("转换为 TextMeshProUGUI", GUILayout.Height(28f)) &&
                TextMeshProUGUIConvertEditor.IsExactTypeSelection(targets, typeof(LocalizedText)))
            {
                TextMeshProUGUIConvertEditor.ConvertBackTargets(targets);
                GUIUtility.ExitGUI();
            }

            DrawPropertiesExcluding(serializedObject, "m_Script", "key");
            if (serializedObject.ApplyModifiedProperties())
            {
                foreach (var t in targets)
                {
                    if (t is LocalizedText loc)
                        loc.ForceUpdateText(true);
                    EditorUtility.SetDirty(t);
                }
            }
        }
    }
}
