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

            LayerID = UILayerID.Main;
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
        }

        public void OnStartBtn()
        {
            Main.ProcedureService.ChangeProcedure(ProcedureType.Main.ToInt());
        }
    }
}