namespace LccModel
{
    public class FsmStartSplash : FsmLaunchStateNode
    {
        public override void OnEnter()
        {
            base.OnEnter();
            BroadcastShowProgress(2);
            ChangeToNextState();
        }
        protected override void ChangeToNextState()
        {
            base.ChangeToNextState();
            _machine.ChangeState<FsmShowLaunchUI>();
        }
    }
}