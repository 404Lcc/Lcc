using Sirenix.OdinInspector.Editor;
using UnityEditor;

namespace LccEditor
{
    public class MenuEditorWindow : AMenuEditorWindow<MenuEditorWindow>
    {
        protected override OdinMenuTree BuildMenuTree()
        {
            AddAEditorWindowBase<FrameworkEditorWindowBase>("框架介绍");
            AddAEditorWindowBase<GeneralEditorWindowBase>("通用功能");
            AddAEditorWindowBase<AssetEditorWindowBase>("资源模式");
            AddAEditorWindowBase<HotfixEditorWindowBase>("热更新模式");
            AddAEditorWindowBase<TagEditorWindowBase>("标签工具");
            AddAEditorWindowBase<LayerEditorWindowBase>("层工具");
            AddAEditorWindowBase<ILRuntimeEditorWindowBase>("ILRuntime工具");
            return OdinMenuTree;
        }
        [MenuItem("Lcc框架/工具箱")]
        public static void ShowFramework()
        {
            OpenEditorWindow("工具箱");
        }
    }
}