namespace LccHotfix
{
    public class GameModel : ViewModelBase
    {
    }
    public class GamePanel : APanelView<GameModel>
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