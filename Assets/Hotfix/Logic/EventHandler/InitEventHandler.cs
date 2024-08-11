namespace LccHotfix
{
    [Event]
    public class InitEventHandler : AEvent<Start>
    {
        protected override void Run(Start data)
        {

            //这里可以初始化


            //进入第一个状态
            SceneStateManager.Instance.GetState(SceneStateType.Login).jumpNode = new JumpNode(PanelType.UILogin);
            SceneStateManager.Instance.ChangeScene(SceneStateType.Login);
        }
    }
}