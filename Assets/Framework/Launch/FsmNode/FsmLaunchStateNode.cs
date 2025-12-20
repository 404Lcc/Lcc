using System.Collections;
using UnityEngine;

namespace LccModel
{
    public abstract class FsmLaunchStateNode : IStateNode
    {
        protected StateMachine _machine;
        protected LauncherOperation _launcherOperation;

        public virtual void OnCreate(StateMachine machine)
        {
            _machine = machine;
            _launcherOperation = machine.Owner as LauncherOperation;
        }

        public virtual void OnEnter()
        {
            Debug.LogWarning($"[Launch] OnEnter {GetType().Name}");

            LaunchEvent.StateChanged.Broadcast(_machine.PreviousNode, _machine.CurrentNode);
        }

        public void BroadcastShowProgress(int index)
        {
            var total = (int)_machine.GetBlackboardValue("total");
            var progress = (float)index / total;
            LaunchEvent.ShowProgress.Broadcast(progress, $"{index}/{total}");
        }

        public virtual void OnUpdate()
        {
        }

        public virtual void OnExit()
        {
            Debug.LogWarning($"[Launch] OnExit {GetType().Name}");
        }

        protected virtual void ChangeToNextState()
        {
        }

        protected Coroutine StartCoroutine(IEnumerator routine)
        {
            return Launcher.Instance.StartCoroutine(routine);
        }
    }
}