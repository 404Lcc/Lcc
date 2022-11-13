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



        public override void OnShow(Panel panel, AObjectBase contextData = null)
        {
            base.OnShow(panel, contextData);
            ShowTopPanel(TopType.CloseBtn, "Game");
        }
    }
}