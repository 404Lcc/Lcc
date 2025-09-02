using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace LccModel
{
    public class FsmStartSplash : IStateNode
    {
        private StateMachine _machine;
        
        public void OnCreate(StateMachine machine)
        {
            _machine = machine;
        }

        public void OnEnter()
        {
            Launcher.Instance.GameControl.SetGameSlow(false);
            Launcher.Instance.GameControl.ChangeFPS();
            Launcher.Instance.GameControl.SetGameSpeed(1);
            Launcher.Instance.GameControl.Resume();
            
            UIForeGroundPanel.Instance.FadeIn(0, null, false, 1, false);
            
            _machine.ChangeState<FsmLoadGameConfig>();
        }

        public void OnUpdate()
        {
        }

        public void OnExit()
        {
        }
    }
}