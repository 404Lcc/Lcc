using UnityEngine;
using UnityEditor;
using UnityEngine.SceneManagement;

namespace ES3Internal
{
    public class SetReferenceID : EditorWindow
    {
        private long id = 0;
        public UnityEngine.Object obj;

        [MenuItem("GameObject/Easy Save 3/Set Reference ID..", false, 33)]
        [MenuItem("Assets/Easy Save 3/Set Reference ID..", false, 33)]

        public static void ShowWindow()
        {
            var selected = Selection.GetFiltered<UnityEngine.Object>(SelectionMode.TopLevel);

            if (selected == null || selected.Length == 0)
                EditorUtility.DisplayDialog("Could not set reference ID", "No reference was selected to set the ID of.", "Ok");
            else if (selected.Length > 1)
                EditorUtility.DisplayDialog("Could not set reference ID", "Multiple references are selected. Please select a single reference.", "Ok");
            else
                EditorWindow.GetWindow<SetReferenceID>("Set Reference ID").obj = selected[0];
        }

        [MenuItem("CONTEXT/Component/Easy Save 3/Set Reference ID..", false, 33)]
        public static void ShowWindowContext(MenuCommand command)
        {
            EditorWindow.GetWindow<SetReferenceID>("Set Reference ID").obj = command.context;
        }

        private void OnGUI()
        {
            GUILayout.Label("Enter new reference ID:", EditorStyles.boldLabel);
            id = EditorGUILayout.LongField("Reference ID", id);

            if (GUILayout.Button("Apply"))
            {
                int setCount = 0;

                string sceneName = null;

                // If this is a scene object, only set the reference ID for the manager in the scene it belongs to.
                if (!EditorUtility.IsPersistent(obj))
                {
                    if (obj is GameObject go)
                        sceneName = go.scene.name;
                    else if (obj is Component c)
                        sceneName = c.gameObject.scene.name;
                }

                for (int i = 0; i < SceneManager.sceneCount; i++)
                {
                    var loadedScene = SceneManager.GetSceneAt(i);

                    if (loadedScene != null && loadedScene.IsValid())
                    {
                        if (sceneName != null && loadedScene.name != sceneName)
                            continue;

                        var mgr = ES3ReferenceMgr.GetManagerFromScene(loadedScene, false);
                        if (mgr != null)
                        {
                            Undo.RecordObject(mgr, "Changed reference ID in manager");
                            mgr.Remove(obj);
                            mgr.Add(obj, id);
                            setCount++;
                        }
                    }
                }

                if (setCount == 0)
                {
                    this.Close();
                    EditorUtility.DisplayDialog("Could not set reference ID", "No open scenes contain reference managers. Add a reference manager by going to Tools > Easy Save 3 > Add Manager to Scene.", "Ok");
                }

                this.Close();
                EditorUtility.DisplayDialog($"Reference ID successfully changed", $"Reference ID changed to {id} in {setCount} managers.", "Ok");
            }
        }

        [MenuItem("GameObject/Easy Save 3/Set Reference ID..", true, 33)]
        [MenuItem("Assets/Easy Save 3/Set Reference ID..", true, 33)]
        private static bool CanSetReference()
        {
            var selected = Selection.GetFiltered<UnityEngine.Object>(SelectionMode.TopLevel);

            return selected != null && selected.Length == 1 && ES3ReferenceMgr.Current != null;
        }
    }
}
