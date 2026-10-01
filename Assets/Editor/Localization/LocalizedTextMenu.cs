using LccHotfix;
using UnityEditor;
using UnityEngine;

namespace LccEditor
{
    public static class LocalizedTextMenu
    {
        [MenuItem("GameObject/UI/本地化文本", false, 1004)]
        public static void CreateLocalizedText()
        {
            if (Selection.activeTransform == null)
                return;

            var obj = new GameObject("localizedText", typeof(RectTransform), typeof(CanvasRenderer));
            obj.AddComponent<LocalizedText>();
            obj.transform.SetParent(Selection.activeTransform, false);
            Selection.activeTransform = obj.transform;
        }
    }
}
