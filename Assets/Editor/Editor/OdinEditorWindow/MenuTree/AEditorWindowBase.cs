using UnityEditor;

namespace LccEditor
{
    public abstract class AEditorWindowBase
    {
        private EditorWindow _editorWindow;
        public EditorWindow EditorWindow => _editorWindow;
        public AEditorWindowBase()
        {
        }
        public AEditorWindowBase(EditorWindow editorWindow)
        {
            this._editorWindow = editorWindow;
        }
        public virtual void OnEnable()
        {
        }
        public virtual void OnDisable()
        {
        }
    }
}