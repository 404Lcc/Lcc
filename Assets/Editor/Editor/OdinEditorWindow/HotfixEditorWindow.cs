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
            Refresh();
        }
        [PropertySpace(10)]
        [LabelText("Mono模式"), Button(ButtonSizes.Gigantic)]
        public void Mono()
        {
            EditorDefine.HotfixMode = HotfixMode.Mono;
            Refresh();
        }
        //[PropertySpace(10)]
        //[LabelText("ILRuntime模式"), Button(ButtonSizes.Gigantic)]
        //public void ILRuntime()
        //{
        //    EditorDefine.HotfixMode = HotfixMode.ILRuntime;
        //    Refresh();
        //}
        [PropertySpace(10)]
        [LabelText("HybridCLR模式"), Button(ButtonSizes.Gigantic)]
        public void HybridCLR()
        {
            EditorDefine.HotfixMode = HotfixMode.HybridCLR;
            Refresh();
        }
        [PropertySpace(10)]
        [LabelText("Release模式"), Button(ButtonSizes.Gigantic)]
        public void Release()
        {
            EditorDefine.IsRelease = true;
            Refresh();
        }
        [PropertySpace(10)]
        [LabelText("Debug模式"), Button(ButtonSizes.Gigantic)]
        public void Debug()
        {
            EditorDefine.IsRelease = false;
            Refresh();
        }

        public void Refresh()
        {
            if (EditorDefine.HotfixMode == HotfixMode.Mono)
            {
                info = "当前是Mono模式";
            }
            //else if (EditorDefine.HotfixMode == HotfixMode.ILRuntime)
            //{
            //    info = "当前是ILRuntime模式";
            //}
            else if (EditorDefine.HotfixMode == HotfixMode.HybridCLR)
            {
                info = "当前是HybridCLR模式";
            }

            if (EditorDefine.IsRelease)
            {
                info += "Release的DLL";
            }
            else
            {
                info += "Debug的DLL";
            }
        }
    }
}