using ET;

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
			Next().Coroutine();
        }
        public void OnUpdate()
		{
		}
        public void OnExit()
		{
		}

		public async ETTask Next()
		{
            await UpdatePanel.Instance.UpdateLoadingPercent(0, 10);

            _machine.ChangeState<FsmInitialize>();
        }

	}
}