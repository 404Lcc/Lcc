namespace LccHotfix
{
    public class MainModel : ViewModelBase
    {
    }
    public class MainPanel : APanelView<MainModel>
    {
        public override void OnInitData(Panel panel)
        {
            base.OnInitData(panel);

            panel.data.type = UIType.Normal;
            panel.data.showMode = UIShowMode.HideOther;
            panel.data.navigationMode = UINavigationMode.NormalNavigation;
        }
    }
}