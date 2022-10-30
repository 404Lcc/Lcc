using UnityEngine.UI;

namespace LccHotfix
{
    public class TopModel : ViewModelBase
    {
    }
    public class TopPanel : APanelView<TopModel>
    {
        public Button closeBtn;
        public override void OnInitData(Panel panel)
        {
            base.OnInitData(panel);

            panel.data.type = UIType.Fixed;
            panel.data.showMode = UIShowMode.Normal;
            panel.data.navigationMode = UINavigationMode.IgnoreNavigation;
        }
        public override void OnRegisterUIEvent(Panel panel)
        {
            base.OnRegisterUIEvent(panel);

            closeBtn.onClick.AddListener(OnClose);
        }
        public void OnClose()
        {
            PanelManager.Instance.PopupNavigationPanel();
        }
    }
}