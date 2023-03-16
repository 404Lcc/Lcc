using LccModel;

namespace LccHotfix
{
    public class GameModel : ViewModelBase
    {
    }
    public class GamePanel : UIPanel<GameModel>
    {
        public override void OnInitComponent(Panel panel)
        {
            base.OnInitComponent(panel);

        }
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
        }
    }
}