using LccHotfix;
using TMPro;
using UnityEditor;
using UnityEngine;

/// <summary>
/// 覆盖 TMP 默认 TextMeshProUGUI Inspector，顶部增加「转换为 LocalizedText」。
/// </summary>
[CustomEditor(typeof(TextMeshProUGUI), true)]
[CanEditMultipleObjects]
public class TextMeshProUGUIConvertEditor : Editor
{
    static MonoScript _localizedTextScript;
    static MonoScript _textMeshProUGUIScript;

    public override void OnInspectorGUI()
    {
        if (IsExactTypeSelection(targets, typeof(TextMeshProUGUI)))
            DrawConvertButton();

        DrawDefaultInspector();
    }

    void DrawConvertButton()
    {
        EditorGUILayout.Space(2f);
        if (GUILayout.Button("转换为 LocalizedText", GUILayout.Height(28f)))
        {
            ConvertTargets(targets);
            GUIUtility.ExitGUI();
        }

        EditorGUILayout.HelpBox(
            "转换为 LocalizedText，保留字体、字号、颜色、对齐、材质等现有设置。",
            UnityEditor.MessageType.Info);
        EditorGUILayout.Space(6f);
    }

    [MenuItem("CONTEXT/TextMeshProUGUI/转换为 LocalizedText")]
    static void ConvertFromContextMenu(MenuCommand command)
    {
        if (command.context is TextMeshProUGUI tmp && tmp.GetType() == typeof(TextMeshProUGUI))
            ConvertTargets(new Object[] { tmp });
    }

    [MenuItem("CONTEXT/TextMeshProUGUI/转换为 LocalizedText", true)]
    static bool ConvertFromContextMenuValidate(MenuCommand command)
    {
        return command.context is TextMeshProUGUI tmp && tmp.GetType() == typeof(TextMeshProUGUI);
    }

    [MenuItem("CONTEXT/LocalizedText/转换为 TextMeshProUGUI")]
    static void ConvertBackFromContextMenu(MenuCommand command)
    {
        if (command.context is LocalizedText loc && loc.GetType() == typeof(LocalizedText))
            ConvertBackTargets(new Object[] { loc });
    }

    [MenuItem("CONTEXT/LocalizedText/转换为 TextMeshProUGUI", true)]
    static bool ConvertBackFromContextMenuValidate(MenuCommand command)
    {
        return command.context is LocalizedText loc && loc.GetType() == typeof(LocalizedText);
    }

    internal static void ConvertTargets(Object[] convertTargets)
    {
        var script = GetLocalizedTextScript();
        if (script == null)
        {
            Debug.LogError("[TextMeshProUGUIConvert] 找不到 LocalizedText 的 MonoScript。");
            return;
        }

        Undo.SetCurrentGroupName("Convert TextMeshProUGUI to LocalizedText");
        var group = Undo.GetCurrentGroup();

        foreach (var t in convertTargets)
        {
            if (t is TextMeshProUGUI tmp && tmp.GetType() == typeof(TextMeshProUGUI))
                ConvertOne(tmp, script, "Convert to LocalizedText");
        }

        Undo.CollapseUndoOperations(group);
        ActiveEditorTracker.sharedTracker.ForceRebuild();
    }

    internal static void ConvertBackTargets(Object[] convertTargets)
    {
        var script = GetTextMeshProUGUIScript();
        if (script == null)
        {
            Debug.LogError("[TextMeshProUGUIConvert] 找不到 TextMeshProUGUI 的 MonoScript。");
            return;
        }

        Undo.SetCurrentGroupName("Convert LocalizedText to TextMeshProUGUI");
        var group = Undo.GetCurrentGroup();

        foreach (var t in convertTargets)
        {
            if (t is LocalizedText loc && loc.GetType() == typeof(LocalizedText))
                ConvertOne(loc, script, "Convert to TextMeshProUGUI");
        }

        Undo.CollapseUndoOperations(group);
        ActiveEditorTracker.sharedTracker.ForceRebuild();
    }

    internal static bool IsExactTypeSelection(Object[] selection, System.Type type)
    {
        if (selection == null || selection.Length == 0)
            return false;

        foreach (var t in selection)
        {
            if (t == null || t.GetType() != type)
                return false;
        }

        return true;
    }

    static void ConvertOne(TextMeshProUGUI source, MonoScript script, string undoName)
    {
        Undo.RegisterCompleteObjectUndo(source, undoName);

        var so = new SerializedObject(source);
        so.Update();
        var scriptProp = so.FindProperty("m_Script");
        if (scriptProp == null)
        {
            Debug.LogError($"[TextMeshProUGUIConvert] {source.name} 缺少 m_Script 字段。", source);
            return;
        }

        scriptProp.objectReferenceValue = script;
        so.ApplyModifiedProperties();
        EditorUtility.SetDirty(source);
    }

    static MonoScript GetLocalizedTextScript()
    {
        if (_localizedTextScript != null)
            return _localizedTextScript;

        _localizedTextScript = LookupMonoScript<LocalizedText>(".__LocalizedTextScriptLookup");
        return _localizedTextScript;
    }

    static MonoScript GetTextMeshProUGUIScript()
    {
        if (_textMeshProUGUIScript != null)
            return _textMeshProUGUIScript;

        _textMeshProUGUIScript = LookupMonoScript<TextMeshProUGUI>(".__TextMeshProUGUIScriptLookup");
        return _textMeshProUGUIScript;
    }

    static MonoScript LookupMonoScript<T>(string tempObjectName) where T : MonoBehaviour
    {
        var go = new GameObject(tempObjectName)
        {
            hideFlags = HideFlags.HideAndDontSave
        };
        try
        {
            var comp = go.AddComponent<T>();
            return MonoScript.FromMonoBehaviour(comp);
        }
        finally
        {
            Object.DestroyImmediate(go);
        }
    }
}
