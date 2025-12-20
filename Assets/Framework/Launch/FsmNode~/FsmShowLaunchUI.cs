using UnityEngine;

namespace LccModel
{
    public class FsmShowLaunchUI : FsmLaunchStateNode
    {
        public override void OnEnter()
        {
            base.OnEnter();
            BroadcastShowProgress(3);
            GameObject prefabPanelUpdate = Resources.Load<GameObject>("Launch/UI/Panel_Update");
            _machine.SetBlackboardValue("BV_LaunchUI", GameObject.Instantiate(prefabPanelUpdate));
            
            LaunchEvent.ShowVersion.Broadcast(GameConfig.GetVersionStr());
            
            ChangeToNextState();
        }
        public override void ChangeToNextState()
        {
            base.ChangeToNextState();
            _machine.ChangeState<FsmRequestVersion>();
        }
    }
}