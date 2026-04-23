namespace LccHotfix
{
    public class FTDelayBhvCfg : ICustomNodeCfg
    {
        //延迟时间
        public FloatCfg TimeLen { get; protected set; }

        public System.Type NodeType()
        {
            return typeof(FTDelayBhv);
        }

        public FTDelayBhvCfg()
        {
            TimeLen = new FloatCfg(0f);
        }

        public FTDelayBhvCfg(float timeLen)
        {
            TimeLen = new FloatCfg(timeLen);
        }

        public FTDelayBhvCfg(string varID)
        {
            TimeLen = new FloatCfg(varID, 0f);
        }
    }

    /// <summary>
    /// 运行时节点:  时间延迟
    /// </summary>
    public class FTDelayBhv : FiniteTimeBhv
    {
        private FTDelayBhvCfg _cfg;

        public override void InitializeNode(ICustomNodeCfg cfg, in CustomNodeContext context)
        {
            base.InitializeNode(cfg, context);
            _cfg = cfg as FTDelayBhvCfg;
        }

        private void Init()
        {
            var timeLen = _cfg.TimeLen.GetValue(this);
            InitDuration(timeLen);
        }


        public override void Reset()
        {
            base.Reset();
            InitDuration(_cfg.TimeLen.GetValue(this));
        }

        protected override void OnBegin()
        {
            Init();
        }
    }
}