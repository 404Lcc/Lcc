using UnityEditor;
using UnityEngine;

namespace ES3Internal
{
    [CustomEditor(typeof(ES3AutoSave))]
    public class ES3AutoSaveEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            if (target == null)
                return;

            var autoSave = (ES3AutoSave)target;

            if (GUILayout.Button("Manage Auto Save Settings"))
                ES3Editor.ES3Window.InitAndShowAutoSave();


            DisplayToggle("saveActive", "active", autoSave == null ? false : autoSave.saveActive);

            if (!PrefabUtility.IsPartOfPrefabAsset(autoSave.transform))
                DisplayToggle("saveDestroyed", "destroyed", autoSave == null ? false : autoSave.saveDestroyed);
            else
                if (EditorGUILayout.ToggleLeft("destroyed", false))
                EditorUtility.DisplayDialog("Marking prefabs destroyed is not necessary", "Marking prefabs as destroyed is not necessary because their destroyed state is implied by their absense from the save data.\nFor example if you destroy a prefab instance and save, it will not be in the save data so will never be created when you load.", "Ok");

            DisplayToggle("saveHideFlags", "hideFlags", autoSave == null ? false : autoSave.saveHideFlags);
            DisplayToggle("saveName", "name", autoSave == null ? false : autoSave.saveName);
            DisplayToggle("saveTag", "tag", autoSave == null ? false : autoSave.saveTag);

            foreach (var component in autoSave.GetComponents<Component>())
            {
                if (component is ES3AutoSave)
                    continue;

                var markedToBeSaved = autoSave.componentsToSave.Contains(component);
                var newMarkedToBeSaved = EditorGUILayout.ToggleLeft(component.GetType().Name, markedToBeSaved);

                if (markedToBeSaved && !newMarkedToBeSaved)
                {
                    Undo.RecordObject(autoSave, "Marked Component to save");
                    autoSave.componentsToSave.Remove(component);
                }

                if (!markedToBeSaved && newMarkedToBeSaved)
                {
                    Undo.RecordObject(autoSave, "Unmarked Component to save");
                    autoSave.componentsToSave.Add(component);
                }
            }

            if (autoSave.componentsToSave.RemoveAll(t => t == null) > 0)
                Undo.RecordObject(autoSave, "Removed null Component from ES3AutoSave");
        }
        void DisplayToggle(string fieldName, string label, bool value)
        {
            if (EditorGUILayout.ToggleLeft(label, value) != value)
                ApplyBool(fieldName, !value);
        }

        void ApplyBool(string propertyName, bool value)
        {
            var autoSave = (ES3AutoSave)target;

            var so = new SerializedObject(autoSave);
            so.FindProperty(propertyName).boolValue = value;
            so.ApplyModifiedProperties();
        }
    }
}