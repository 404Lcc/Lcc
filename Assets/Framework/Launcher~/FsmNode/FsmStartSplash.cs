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