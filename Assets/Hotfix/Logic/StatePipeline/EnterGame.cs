namespace LccHotfix
{
    [StatePipeline(SceneStateType.Main, SceneStateType.Game)]
    public class EnterGame : StatePipeline
    {
        public override bool CheckState()
        {
            MainPanel mainPanel = (MainPanel)PanelManager.Instance.GetPanelLogic(PanelType.Main);
            return mainPanel.ViewModel.isEnterGame;
        }
    }
}