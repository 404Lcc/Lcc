namespace LccHotfix
{
    public delegate bool NodeParamCndFunction(CustomNode node);

    public class DelegateConditionCfg : ConditionBaseCfg
    {
        public NodeParamCndFunction CndFunc { get; protected set; }

        public override System.Type NodeType()
        {
            return typeof(DelegateCondition);
        }


        public DelegateConditionCfg(NodeParamCndFunction cndFunc)
        {
            CndFunc = cndFunc;
        }
    }

    /// <summary>
    /// 运行时：调用简单函数
    /// </summary>
    public class DelegateCondition : ConditionNodeBase
    {
        private DelegateConditionCfg _cfg;

        public override void InitializeNode(ICustomNodeCfg cfg, in CustomNodeContext context)
        {
            base.InitializeNode(cfg, context);
            _cfg = cfg as DelegateConditionCfg;
        }

        public override void Destroy()
        {
            _cfg = null;
            base.Destroy();
        }


        protected override bool Inner_ConditionCheck()
        {
            if (_cfg?.CndFunc != null)
            {
                return _cfg.CndFunc(this);
            }

            return false;
        }
    }
}