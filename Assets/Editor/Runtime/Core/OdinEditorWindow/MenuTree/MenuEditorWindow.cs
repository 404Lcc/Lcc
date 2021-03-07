using Sirenix.OdinInspector.Editor;
using UnityEditor;

namespace LccEditor
{
    public class MenuEditorWindow : AMenuEditorWindow<MenuEditorWindow>
    {
        protected override OdinMenuTree BuildMenuTree()
        {
            AddAEditorWindowBase<FrameworkEditorWindowBase>("框架介绍");
            return OdinMenuTree;
        }
        [MenuItem("Lcc框架/工具箱")]
        public static void ShowFramework()
        {
            OpenEditorWindow("工具箱");
        }
    }
}