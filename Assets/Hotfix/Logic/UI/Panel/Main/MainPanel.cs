using LccModel;
using UnityEngine.UI;

namespace LccHotfix
{
    public class MainModel : ViewModelBase
    {
    }
    public class MainPanel : APanelView<MainModel>
    {
        public Button testBtn;
        public override void OnInitData(Panel panel)
        {
            base.OnInitData(panel);

            panel.data.type = UIType.Normal;
            panel.data.showMode = UIShowMode.HideOther;
            panel.data.navigationMode = UINavigationMode.IgnoreNavigation;
        }
        public override void OnShow(Panel panel, AObjectBase contextData = null)
        {
            base.OnShow(panel, contextData);
            ShowTopPanel(TopType.CloseBtn | TopType.Gold, "Main");
        }
        public override void OnRegisterUIEvent(Panel panel)
        {
            testBtn.onClick.AddListener(OnTest);
        }

        public void OnTest()
        {
            PanelManager.Instance.ShowPanel(PanelType.Game);
        }
    }
}