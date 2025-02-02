using UnityEditor;

namespace LccEditor
{
    public class MenuEditorWindow : AMenuEditorWindow<MenuEditorWindow>
    {
        protected override void OnEnable()
        {
            AddAEditorWindowBase<FrameworkEditorWindow>("框架介绍");
            AddAEditorWindowBase<GeneralEditorWindow>("通用功能");
            AddAEditorWindowBase<HotfixEditorWindow>("热更新模式");
            AddAEditorWindowBase<TagEditorWindow>("标签工具");
            AddAEditorWindowBase<LayerEditorWindow>("层工具");
            AddAEditorWindowBase<UIEditorWindow>("UI工具");
            AddAEditorWindowBase<GameConfigEditorWindow>("框架配置");
        }
        [MenuItem("Lcc框架/工具箱")]
        public static void ShowFramework()
        {
            OpenEditorWindow("工具箱");
        }
    }
}