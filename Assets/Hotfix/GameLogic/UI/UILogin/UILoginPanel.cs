using LccModel;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace LccHotfix
{
    public class UILoginPanel : UIElementBase, ICoroutine
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
        }






        public void OnStartBtn()
        {
            Main.UIService.ShowElement(UIPanelDefine.UIBattlePanel, null);
            // Main.ProcedureService.ChangeProcedure(ProcedureType.Main.ToInt());
        }
    }
}