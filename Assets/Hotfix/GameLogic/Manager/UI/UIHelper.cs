using System.Collections.Generic;

namespace LccHotfix
{
    public class UIHelper : IUIHelper
    {
        private Dictionary<string, WindowMode> _dict = new Dictionary<string, WindowMode>();

        public UIHelper()
        {
            foreach (var item in Main.ConfigService.Tables.TBPanel.DataList)
            {
                WindowMode mode = new WindowMode();
                mode.prefabName = item.PrefabName;
                mode.depth = item.Depth;
                mode.rootName = item.RootName;
                mode.logicName = item.LogicName;
                mode.bgTex = item.BgTex;
                mode.sound = item.Sound;
                mode.openAnim = item.OpenAnim;
                mode.showScreenMask = item.ShowScreenMask;
                mode.windowFlag = item.WindowFlag;
                mode.rejectFlag = item.RejectFlag;
                mode.escapeType = (EscapeType)item.EscapeType;
                mode.releaseType = (ReleaseType)item.ReleaseType;
                mode.returnNodeType = item.ReturnNodeType;
                mode.returnNodeName = item.ReturnNodeName;
                mode.returnNodeParam = item.ReturnNodeParam;
                _dict.Add(item.PrefabName, mode);
            }

            // _dict.Add(UIWindowDefine.UILoginPanel, new WindowMode()
            // {
            //     prefabName = UIWindowDefine.UILoginPanel,
            //     depth = 0,
            //     logicName = UIWindowDefine.UILoginPanel,
            //     bgTex = "",
            //     openAnim = false,
            //     sound = 0,
            //     showScreenMask = 0,
            //     escapeType = EscapeType.AUTO_CLOSE,
            //     releaseType = ReleaseType.AUTO,
            //     rejectFlag = (int)RejectFlag.NONE,
            //     windowFlag = (int)NodeFlag.FULL_SCREEN,
            //     rootName = UIRootDefine.UIRootLogin,
            //     returnNodeName = "",
            //     returnNodeType = (int)NodeType.WINDOW,
            //     returnNodeParam = 0
            // });
            //
            // _dict.Add(UIWindowDefine.UIMainPanel, new WindowMode()
            // {
            //     prefabName = UIWindowDefine.UIMainPanel,
            //     depth = 0,
            //     logicName = UIWindowDefine.UIMainPanel,
            //     bgTex = "",
            //     openAnim = false,
            //     sound = 0,
            //     showScreenMask = 0,
            //     escapeType = EscapeType.AUTO_CLOSE,
            //     releaseType = ReleaseType.AUTO,
            //     rejectFlag = (int)RejectFlag.MAIN,
            //     windowFlag = (int)NodeFlag.FULL_SCREEN,
            //     rootName = UIRootDefine.UIRootMain,
            //     returnNodeName = "",
            //     returnNodeType = (int)NodeType.WINDOW,
            //     returnNodeParam = 0
            // });
        }

        public WindowMode GetWindowMode(string windowName)
        {
            if (_dict.TryGetValue(windowName, out var windowMode))
            {
                return windowMode;
            }

            return default;
        }
    }
}