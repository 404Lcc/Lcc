using UnityEngine.UI;

namespace LccHotfix
{
    public class UIMainPanel : UIElementBase
    {
        public Button startBtn;
        public override void OnConstruct()
        {
            base.OnConstruct();
            
            LayerID = UILayerID.Main;
            IsFullScreen = true;
            EscapeType = EscapeType.Hide;
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
            Main.ProcedureService.ChangeProcedure(ProcedureType.Battle.ToInt());
        }
    }
}