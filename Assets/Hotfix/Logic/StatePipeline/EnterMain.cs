namespace LccHotfix
{
    [StatePipeline(SceneStateType.Login, SceneStateType.Main)]
    public class EnterMain : StatePipeline
    {
        public override bool CheckState()
        {
            LoginPanel loginPanel = (LoginPanel)PanelManager.Instance.GetPanelLogic(PanelType.Login);
            return loginPanel./*ViewModel.*/isEnterMain;
        }
    }
}