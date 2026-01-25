using UnityEngine;

namespace LccHotfix
{
    public class GuideState : IGuideStateNode
    {
        protected GuideFSM _fsm;
        protected GuideStateData _data;
        private float _curTime = 0;

        public virtual void OnCreate(GuideFSM machine)
        {
            _fsm = machine;
            _data = machine.GetBlackboardValue("data") as GuideStateData;
            _curTime = 0;
        }

        public virtual void OnEnter()
        {
            _curTime = 0;
        }

        public virtual void OnUpdate()
        {
            if (_data.Config.timeout != -1)
            {
                if (_data.IsPause)
                {
                    return;
                }

                if (_curTime >= _data.Config.timeout)
                {
                    _data.IsTimeout = true;
                    return;
                }
            }

            _curTime += Time.unscaledDeltaTime;
        }

        public virtual void OnExit()
        {
            _curTime = 0;
        }
    }
}