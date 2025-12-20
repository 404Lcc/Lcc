namespace LccModel
{
    public class FsmStartSplash : FsmLaunchStateNode
    {
        public override void OnEnter()
        {
            base.OnEnter();
            ChangeToNextState();
        }
    }
}