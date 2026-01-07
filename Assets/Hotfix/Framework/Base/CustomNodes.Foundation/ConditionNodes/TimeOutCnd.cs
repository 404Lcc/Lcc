using System.Xml;

namespace LccHotfix
{
    public static partial class NodeConfigTypeRegistry
    {
        static bool _TimeOutCndCfg = Register(typeof(TimeOutCndCfg), NodeCategory.Cnd);
    }

    //静态配置
    public class TimeOutCndCfg : ConditionBaseCfg
    {
        public override System.Type NodeType()
        {
            return typeof(TimeOutCnd);
        }

        //定时设置（秒）
        public float TimeLimit;

        public override bool ParseFromXml(XmlNode cndNode)
        {
            string time = XmlHelper.GetAttribute(cndNode, "time");
            if (CLHelper.Assert(!string.IsNullOrEmpty(time)))
            {
                float.TryParse(time, out TimeLimit);
            }

            return base.ParseFromXml(cndNode);
        }
    }

    /// <summary>
    /// 时间条件，超时
    /// </summary>
    public class TimeOutCnd : ConditionNodeBase, INeedStopCheck, INeedUpdate
    {
        private TimeOutCndCfg mCfg;
        private float mTimeAcc = 0;

        public void Init(TimeOutCndCfg cfg)
        {
            mCfg = cfg;
            mTimeAcc = 0;
        }

        public override void InitializeNode(ICustomNodeCfg cfg, in CustomNodeContext context)
        {
            base.InitializeNode(cfg, context);
            Init(cfg as TimeOutCndCfg);
        }

        public override void Destroy()
        {
            base.Destroy();
            mTimeAcc = 0;
            mCfg = null;
        }

        public virtual float Update(float dt)
        {
            if (mTimeAcc <= mCfg.TimeLimit)
            {
                mTimeAcc += dt;
            }

            return dt;
        }

        protected override bool Inner_ConditionCheck()
        {
            return mTimeAcc > mCfg.TimeLimit;
        }

        public override void Reset()
        {
            mTimeAcc = 0;
        }

        public bool CanStop()
        {
            return IsConditionReached();
        }
    }
}