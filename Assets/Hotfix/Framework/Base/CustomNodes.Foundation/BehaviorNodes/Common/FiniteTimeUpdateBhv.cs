namespace LccHotfix
{
    public static partial class NodeConfigTypeRegistry
    {
        static bool _DelayUpdateBhvCfg = Register(typeof(FiniteTimeUpdateCfg), NodeCategory.Bhv);
    }

    public class FiniteTimeUpdateCfg : ICustomNodeCfg
    {
        //延迟时间
        public FloatCfg TimeLen { get; protected set; }
        public NodeParamTickAction TickAction { get; protected set; }

        public System.Type NodeType()
        {
            return typeof(FiniteTimeUpdateBhv);
        }

        public FiniteTimeUpdateCfg()
        {
            TimeLen = new FloatCfg(0f);
        }

        public FiniteTimeUpdateCfg(float timeLen)
        {
            TimeLen = new FloatCfg(timeLen);
        }

        public FiniteTimeUpdateCfg(string varID)
        {
            TimeLen = new FloatCfg(varID, 0f);
        }

        public FiniteTimeUpdateCfg WithUpdate(NodeParamTickAction tickAction)
        {
            TickAction = tickAction;
            return this;
        }

    }

    /// <summary>
    /// 运行时节点:  时间延迟
    /// </summary>
    public class FiniteTimeUpdateBhv : FiniteTimeBhv
    {
        private FiniteTimeUpdateCfg mCfg;


        public override void InitializeNode(ICustomNodeCfg cfg, in CustomNodeContext context)
        {
            base.InitializeNode(cfg, context);
            mCfg = cfg as FiniteTimeUpdateCfg;
        }

        private void Init()
        {
            var timeLen = mCfg.TimeLen.GetValue(this);
            InitDuration(timeLen);
        }

        public override void Destroy()
        {
            base.Destroy();
        }

        public override void Reset()
        {
            base.Reset();
            InitDuration(mCfg.TimeLen.GetValue(this));
        }

        protected override void OnBegin()
        {
            Init();
        }

        public override float Update(float dt)
        {
            mCfg.TickAction.Invoke(this, dt);
            return base.Update(dt);
        }
    }
}