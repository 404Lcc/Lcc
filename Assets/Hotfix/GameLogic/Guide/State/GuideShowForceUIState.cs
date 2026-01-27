using LccModel;

namespace LccHotfix
{
    public class GuideShowForceUIState : GuideState
    {
        public override void OnEnter()
        {
            base.OnEnter();
            var args = _data.Config.defaultStateArgs;
            if (args == null || args.Count <= 0)
            {
                return;
            }

            GameUtility.ShowForceGuide(args[0]);
            GameUtility.AddHandle<EvtClickForceGuideFinish>(OnEvtClickForceGuideFinish);
        }

        private void OnEvtClickForceGuideFinish(IEventMessage obj)
        {
            _fsm.ChangeState<GuideFinishState>();
        }

        public override void OnExit()
        {
            base.OnExit();
            GameUtility.RemoveHandle<EvtClickForceGuideFinish>(OnEvtClickForceGuideFinish);
        }
    }
}