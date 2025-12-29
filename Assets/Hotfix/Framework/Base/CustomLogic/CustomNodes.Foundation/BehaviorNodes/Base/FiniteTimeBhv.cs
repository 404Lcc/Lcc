namespace LccHotfix
{
    /// <summary>
    /// 能在有限固定时间内结束的行为
    /// </summary>
    public abstract class FiniteTimeBhv : BehaviorNodeBase, INeedStopCheck
    {
        //行为的持续时间
        private float mDuration = 0;
        private float mCfgDuration = 0;
        private bool mIsEnd = false;

        public virtual bool IsDurationEnd()
        {
            return mDuration <= 0;         
        }   

        public float GetDuration() 
        {
            if (mDuration < 0)
                return 0;
            return mDuration; 
        }

        protected void InitDuration(float duration) 
        {
            if (duration < 0)
            {
                duration = 0;
            }
            mDuration = duration;
            mCfgDuration = duration;
            mIsEnd = false;
        }

        protected virtual void OnDurationEnd() { }


        public override void Reset()
        {
            base.Reset();
            mDuration = mCfgDuration;
            mIsEnd = false;
        } 

        public override float Update(float dt)
        {
            if (!mHasUpdate)
            {
                mHasUpdate = true;
                OnBegin();
            }

            var dt_remain = dt;
            
            if (!IsDurationEnd())
            {
                if (mDuration > 0 && dt > mDuration)
                {
                    base.Update(mDuration);
                    dt_remain = dt - mDuration;
                }
                else
                {
                    base.Update(dt);
                    dt_remain = 0;
                }

                mDuration -= dt;
            }

            if (IsDurationEnd() && !mIsEnd)
            {
                mIsEnd = true;
                OnDurationEnd();
            }
            return dt_remain;
        }

        public override void Destroy()
        {
            mDuration = 0;
            mCfgDuration = 0;
            mIsEnd = false;
            base.Destroy();
        }

        public virtual bool CanStop()
        {
            return mIsEnd;
        }
    }

}