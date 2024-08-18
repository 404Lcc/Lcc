using LccModel;
using Sirenix.OdinInspector;
using UnityEditor;

namespace LccEditor
{
    public class HotfixEditorWindow : AEditorWindowBase
    {
        [PropertySpace(10)]
        [HideLabel, DisplayAsString]
        public string info;
        public HotfixEditorWindow()
        {
        }
        public HotfixEditorWindow(EditorWindow editorWindow) : base(editorWindow)
        {
        }
    }
}