using System.Collections;

namespace LccModel
{
    public class FsmPatchPrepare : IStateNode
    {
        private StateMachine _machine;

        public void OnCreate(StateMachine machine)
        {
            _machine = machine;
        }
        public void OnEnter()
        {
            Launcher.Instance.StartCoroutine(Next());
        }
        public void OnUpdate()
        {
        }
        public void OnExit()
        {
        }

        public IEnumerator Next()
        {
            UILoadingPanel.Instance.UpdateLoadingPercent(61, 70);

            yield return null;

            _machine.ChangeState<FsmInitialize>();
        }

    }
}