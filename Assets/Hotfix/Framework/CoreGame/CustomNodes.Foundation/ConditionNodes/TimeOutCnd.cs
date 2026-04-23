namespace LccHotfix
{
    //静态配置
    public class TimeOutCndCfg : ConditionBaseCfg
    {

        //定时设置（秒）
        public float TimeLimit { get; set; }

        public override System.Type NodeType()
        {
            return typeof(TimeOutCnd);
        }

    }

    /// <summary>
    /// 时间条件，超时
    /// </summary>
    public class TimeOutCnd : ConditionNodeBase, INeedStopCheck, INeedUpdate
    {
        private TimeOutCndCfg _cfg;
        private float _timeAcc = 0;

        public void Init(TimeOutCndCfg cfg)
        {
            _cfg = cfg;
            _timeAcc = 0;
        }

        public override void InitializeNode(ICustomNodeCfg cfg, in CustomNodeContext context)
        {
            base.InitializeNode(cfg, context);
            Init(cfg as TimeOutCndCfg);
        }

        public override void Destroy()
        {
            base.Destroy();
            _timeAcc = 0;
            _cfg = null;
        }

        public virtual float Update(float dt)
        {
            if (_timeAcc <= _cfg.TimeLimit)
            {
                _timeAcc += dt;
            }

            return dt;
        }

        protected override bool Inner_ConditionCheck()
        {
            return _timeAcc > _cfg.TimeLimit;
        }

        public override void Reset()
        {
            _timeAcc = 0;
        }

        public bool CanStop()
        {
            return IsConditionReached();
        }
    }
}