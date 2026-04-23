namespace LccHotfix
{
    /// <summary>
    /// 能在有限固定时间内结束的行为
    /// </summary>
    public abstract class FiniteTimeBhv : BehaviorNodeBase, INeedStopCheck
    {
        //行为的持续时间
        private float _duration = 0;
        private float _cfgDuration = 0;
        private bool _isEnd = false;

        public virtual bool IsDurationEnd()
        {
            return _duration <= 0;
        }

        public float GetDuration()
        {
            if (_duration < 0)
                return 0;
            return _duration;
        }

        protected void InitDuration(float duration)
        {
            if (duration < 0)
            {
                duration = 0;
            }

            _duration = duration;
            _cfgDuration = duration;
            _isEnd = false;
        }

        protected virtual void OnDurationEnd()
        {
        }

        public override void Reset()
        {
            base.Reset();
            _duration = _cfgDuration;
            _isEnd = false;
        }

        public override float Update(float dt)
        {
            if (!_hasUpdate)
            {
                _hasUpdate = true;
                OnBegin();
            }

            var dt_remain = dt;

            if (!IsDurationEnd())
            {
                if (_duration > 0 && dt > _duration)
                {
                    base.Update(_duration);
                    dt_remain = dt - _duration;
                }
                else
                {
                    base.Update(dt);
                    dt_remain = 0;
                }

                _duration -= dt;
            }

            if (IsDurationEnd() && !_isEnd)
            {
                _isEnd = true;
                OnDurationEnd();
            }

            return dt_remain;
        }

        public override void Destroy()
        {
            _duration = 0;
            _cfgDuration = 0;
            _isEnd = false;
            base.Destroy();
        }

        public virtual bool CanStop()
        {
            return _isEnd;
        }
    }
}