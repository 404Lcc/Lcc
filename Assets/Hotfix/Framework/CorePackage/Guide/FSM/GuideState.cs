using UnityEngine;

namespace LccHotfix
{
    public class GuideState : IGuideStateNode
    {
        protected GuideFSM _fsm;
        private float _curTime = 0;

        public virtual void OnCreate(GuideFSM machine)
        {
            _fsm = machine;
            _curTime = 0;
        }

        public virtual void OnEnter()
        {
            _curTime = 0;
        }

        public virtual void OnUpdate()
        {
            _curTime += Time.unscaledDeltaTime;
        }

        public virtual void OnExit()
        {
            _curTime = 0;
        }
    }
}