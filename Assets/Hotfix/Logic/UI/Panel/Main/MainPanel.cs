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
            panel.data.navigationMode = UINavigationMode.NormalNavigation;
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