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
            Event.Instance.AddListener(EventType.InitializeFailed, UpdatePanel.Instance);
            Event.Instance.AddListener(EventType.PatchStatesChange, UpdatePanel.Instance);
            Event.Instance.AddListener(EventType.FoundUpdateFiles, UpdatePanel.Instance);
            Event.Instance.AddListener(EventType.DownloadProgressUpdate, UpdatePanel.Instance);
            Event.Instance.AddListener(EventType.PackageVersionUpdateFailed, UpdatePanel.Instance);
            Event.Instance.AddListener(EventType.PatchManifestUpdateFailed, UpdatePanel.Instance);
            Event.Instance.AddListener(EventType.WebFileDownloadFailed, UpdatePanel.Instance);

            await UpdatePanel.Instance.UpdateLoadingPercent(0, 10);

            _machine.ChangeState<FsmInitialize>();
        }

	}
}