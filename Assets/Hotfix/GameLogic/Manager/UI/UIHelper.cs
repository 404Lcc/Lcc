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
                _dict.Add(item.PrefabName, mode);
            }
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