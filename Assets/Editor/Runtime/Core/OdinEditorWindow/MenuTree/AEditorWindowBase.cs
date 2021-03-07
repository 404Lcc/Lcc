using Sirenix.OdinInspector;
using UnityEditor;

namespace LccEditor
{
    public abstract class AEditorWindowBase
    {
        [HideInEditorMode]
        public EditorWindow editorWindow;
        public AEditorWindowBase()
        {
        }
        public AEditorWindowBase(EditorWindow editorWindow)
        {
            this.editorWindow = editorWindow;
        }
        public virtual void OnEnable()
        {
        }
        public virtual void OnDisable()
        {
        }
    }
}