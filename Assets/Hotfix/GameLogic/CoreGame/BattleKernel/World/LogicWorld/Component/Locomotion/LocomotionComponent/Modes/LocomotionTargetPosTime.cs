using UnityEngine;

namespace LccHotfix
{
    public class LocomotionTargetPosTime : LocomotionBase
    {
        public float Duration;
        public Vector3 TargetPos;

        private float mPassedTime;
        private Vector3 mStartPos;
        private bool mInitStartPos;

        public void Init(float time, Vector3 targetPos)
        {
            Duration = time;
            TargetPos = targetPos;
            mPassedTime = 0;
            mInitStartPos = false;
        }
        
        public override void Update(float dt, LogicEntity e)
        {
            if (!mInitStartPos)
            {
                mInitStartPos = true;
                mStartPos = e.comTransform.position;
            }

            if (!mInitStartPos)
            {
                return;
            }

            if (mPassedTime >= Duration)
            {
                e.SetPosition(TargetPos);
                return;
            }
            
            if (!e.hasComTransform)
                return;
            var newPos = Vector3.Lerp(mStartPos, TargetPos, mPassedTime);
            mPassedTime += Time.deltaTime;
            e.SetPosition(newPos);
        }
    }
}
