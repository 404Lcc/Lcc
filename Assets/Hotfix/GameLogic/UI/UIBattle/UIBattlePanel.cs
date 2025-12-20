using System.Collections.Generic;
using UnityEngine.UI;

namespace LccHotfix
{
    public class UIBattlePanel : UIElementBase, ICoroutine
    {
        public Button startBtn;
        public override void OnConstruct()
        {
            base.OnConstruct();

            IsFullScreen = true;
        }

        public override void OnCreate()
        {
            base.OnCreate();
            startBtn.onClick.AddListener(OnStartBtn);

        }

        public override void OnShow(object[] paramsList)
        {
            base.OnShow(paramsList);
            Main.UIService.ShowElement(UIPanelDefine.UIMainPanel, null);
            // Main.WindowService.OpenRoot(UIRootDefine.UIRootBattle, null);
        }




        public void OnStartBtn()
        {
            Main.UIService.ShowElement(UIPanelDefine.UIMainPanel, null);
        }
    }
}