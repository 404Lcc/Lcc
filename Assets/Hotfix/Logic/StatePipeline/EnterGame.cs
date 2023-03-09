namespace LccHotfix
{
    [StatePipeline(SceneStateName.Main, SceneStateName.Game)]
    public class EnterGame : StatePipeline
    {
        public EnterGame(string sceneName, string target) : base(sceneName, target)
        {
        }

        public override bool CheckState()
        {
            MainPanel mainPanel = (MainPanel)PanelManager.Instance.GetPanelLogic(PanelType.Main);
            return mainPanel.ViewModel.isEnterGame;
        }
    }
}