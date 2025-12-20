using System.Collections;
using UnityEngine;

namespace LccModel
{
    public abstract class FsmLaunchStateNode : IStateNode
    {
        private int _nodeIndex;
        protected StateMachine _machine;
        protected LauncherOperation _launcherOperation;

        public void Initialize(params object[] args)
        {
            _nodeIndex = (int)args[0];
        }

        public virtual void OnCreate(StateMachine machine)
        {
            _machine = machine;
            _launcherOperation = machine.Owner as LauncherOperation;
        }

        public virtual void OnEnter()
        {
            Debug.LogWarning($"[Launch]OnEnter {GetType().Name}");

            LaunchEvent.StateChanged.Broadcast(_machine.PreviousNode, _machine.CurrentNode);
            
            var total = _machine.NodeCount;
            var progress = (float)_nodeIndex / total;
            LaunchEvent.ShowProgress.Broadcast(progress, $"{_nodeIndex}/{total}");
        }

        public virtual void OnUpdate()
        {
        }

        public virtual void OnExit()
        {
            Debug.LogWarning($"[Launch]OnExit {GetType().Name}");
        }

        protected void ChangeToNextState()
        {
            _machine.ChangeToNextState();
        }

        protected Coroutine StartCoroutine(IEnumerator routine)
        {
            return Launcher.Instance.StartCoroutine(routine);
        }
    }
}