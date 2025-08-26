using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace LccModel
{
    /// <summary>
    /// 下载完毕
    /// </summary>
    public class FsmDownloadPackageOver : IStateNode
    {
        private StateMachine _machine;

        public void OnCreate(StateMachine machine)
        {
            _machine = machine;
        }
        public void OnEnter()
        {
            _machine.ChangeState<FsmClearCacheBundle>();
        }
        public void OnUpdate()
        {
        }
        public void OnExit()
        {
        }
    }
}