using System.Collections.Generic;

namespace LccHotfix
{
    public class UIRootDefine
    {
        public const string UIRootLogin = "UIRootLogin";
        public const string UIRootMain = "UIRootMain";
    }
    public class UIWindowDefine
    {
        public const string UILoginPanel = "UILoginPanel";
        public const string UIMainPanel = "UIMainPanel";
    }
    public class UIWindowModeDefine
    {
        private Dictionary<string, WindowMode> _dict = new Dictionary<string, WindowMode>();

        public UIWindowModeDefine()
        {
            _dict.Add(UIWindowDefine.UILoginPanel, new WindowMode()
            {
                prefabName = UIWindowDefine.UILoginPanel,
                depth = 0,
                logicName = UIWindowDefine.UILoginPanel,
                bgTex = "",
                openAnim = false,
                sound = 0,
                showScreenMask = 0,
                escapeType = EscapeType.AUTO_CLOSE,
                releaseType = ReleaseType.AUTO,
                rejectFlag = (int)RejectFlag.NONE,
                windowFlag = (int)NodeFlag.FULL_SCREEN,
                rootName = UIRootDefine.UIRootLogin,
                returnNodeName = "",
                returnNodeType = (int)NodeType.WINDOW,
                returnNodeParam = 0
            });

            _dict.Add(UIWindowDefine.UIMainPanel, new WindowMode()
            {
                prefabName = UIWindowDefine.UIMainPanel,
                depth = 0,
                logicName = UIWindowDefine.UIMainPanel,
                bgTex = "",
                openAnim = false,
                sound = 0,
                showScreenMask = 0,
                escapeType = EscapeType.AUTO_CLOSE,
                releaseType = ReleaseType.AUTO,
                rejectFlag = (int)RejectFlag.MAIN,
                windowFlag = (int)NodeFlag.FULL_SCREEN,
                rootName = UIRootDefine.UIRootMain,
                returnNodeName = "",
                returnNodeType = (int)NodeType.WINDOW,
                returnNodeParam = 0
            });
        }

        public WindowMode Get(string windowName)
        {
            if (_dict.TryGetValue(windowName, out var windowMode))
            {
                return windowMode;
            }
            return default;
        }
    }
}