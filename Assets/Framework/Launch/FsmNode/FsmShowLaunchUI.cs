using UnityEngine;

namespace LccModel
{
    public class FsmShowLaunchUI : FsmLaunchStateNode
    {
        public override void OnEnter()
        {
            base.OnEnter();
            GameObject prefabPanelUpdate = Resources.Load<GameObject>("Launch/UI/Panel_Update");
            _machine.SetBlackboardValue("BV_LaunchUI", GameObject.Instantiate(prefabPanelUpdate));
            
            LaunchEvent.ShowVersion.Broadcast(AppConfig.GetVersionStr());
            
            ChangeToNextState();
        }
    }
}