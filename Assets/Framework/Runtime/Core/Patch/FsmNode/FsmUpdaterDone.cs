namespace LccModel
{
    /// <summary>
    /// 流程更新完毕
    /// </summary>
    public class FsmUpdaterDone : IStateNode
    {
        void IStateNode.OnCreate(StateMachine machine)
        {
        }
        void IStateNode.OnEnter()
        {
            UpdatePanel.Instance.UpdateLoadingPercent(90, 100);
        }
        void IStateNode.OnUpdate()
        {
        }
        void IStateNode.OnExit()
        {
        }
    }
}