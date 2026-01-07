using System.Xml;

namespace LccHotfix
{
    public static partial class NodeConfigTypeRegistry
    {
        static bool _DelayBhvCfg = Register(typeof(FTDelayBhvCfg), NodeCategory.Bhv);
    }

    public class FTDelayBhvCfg : ICustomNodeCfg, IParseFromXml
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

        public bool ParseFromXml(XmlNode xmlNode)
        {
            var str = XmlHelper.GetAttribute(xmlNode, "TimeLen");
            CLHelper.Assert(!string.IsNullOrEmpty(str));
            TimeLen = new FloatCfg(0);
            return TimeLen.ParseByFormatString(str);
        }
    }

    /// <summary>
    /// 运行时节点:  时间延迟
    /// </summary>
    public class FTDelayBhv : FiniteTimeBhv
    {
        private FTDelayBhvCfg mCfg;

        public override void InitializeNode(ICustomNodeCfg cfg, in CustomNodeContext context)
        {
            base.InitializeNode(cfg, context);
            mCfg = cfg as FTDelayBhvCfg;
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
    }
}