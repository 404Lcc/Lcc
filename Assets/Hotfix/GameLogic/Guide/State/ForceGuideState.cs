using LccModel;

namespace LccHotfix
{
    public class GuideOpenUIState : GuideState
    {
        public string name;

        public override void OnEnter()
        {
            base.OnEnter();

            var args = _data.Config.defaultStateArgs;
            if (args == null || args.Count <= 0)
            {
                return;
            }

            name = args[0];
            Main.UIService.ShowElement(name);
        }

        public override void OnUpdate()
        {
            base.OnUpdate();

            if (!string.IsNullOrEmpty(name) && Main.UIService.IsElementActive(name))
            {
                _fsm.ChangeState<GuideFinishState>();
            }
        }


        public override void OnExit()
        {
            base.OnExit();
            name = "";
        }
    }

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