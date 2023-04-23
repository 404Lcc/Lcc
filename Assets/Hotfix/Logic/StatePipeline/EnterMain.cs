namespace LccHotfix
{
    [StatePipeline(SceneStateName.Login, SceneStateName.Main)]
    public class EnterMain : StatePipeline
    {
        public EnterMain(string sceneName, string target) : base(sceneName, target)
        {
        }

        public override bool CheckState()
        {
            LoginPanel loginPanel = (LoginPanel)PanelManager.Instance.GetPanelLogic(PanelType.Login);
            return loginPanel./*ViewModel.*/isEnterMain;
        }
    }
}