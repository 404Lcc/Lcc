using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using UnityEngine;

namespace LccModel
{
    public class FsmRequestVersion : FsmLaunchStateNode
    {
        public override void ChangeToNextState()
        {
            base.ChangeToNextState();
            BroadcastShowProgress(4);
            _machine.ChangeState<FsmInitializePackage>();
        }
    }
}