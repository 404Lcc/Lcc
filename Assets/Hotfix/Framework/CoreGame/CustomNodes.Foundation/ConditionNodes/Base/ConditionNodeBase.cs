namespace LccHotfix
{
    public abstract class ConditionBaseCfg : ICustomNodeCfg
    {
        //是否对条件结果取反
        public bool UseUnaryOperationNOT { get; protected set; } = false;
        //保存判断结果 for UseFixedResult
        public bool UseFixedResult { get; protected set; } = false;

        public abstract System.Type NodeType();
    }

    public abstract class ConditionNodeBase : CustomNode, ICondition
    {
        //是否对条件结果取反
        protected bool _useUnaryOperationNot = false;

        //是否判断一次之后，保存结果，使得返回值不再变化
        protected bool _useFixedResult = false;

        //保存判断结果 for UseFixedResult
        protected bool? _fixedResult = null;


        public override void InitializeNode(ICustomNodeCfg cfg, in CustomNodeContext context)
        {
            base.InitializeNode(cfg, context);
            var theCfg = cfg as ConditionBaseCfg;
            _useUnaryOperationNot = theCfg.UseUnaryOperationNOT;
            _useFixedResult = theCfg.UseFixedResult;
            _fixedResult = null;
        }

        public override void Destroy()
        {
            _useUnaryOperationNot = false;
            _useFixedResult = false;
            _fixedResult = null;
            base.Destroy();
        }


        public virtual bool IsConditionReached()
        {
            if (_fixedResult != null)
            {
                return (bool)_fixedResult;
            }

            var res = Inner_ConditionCheck();
            if (_useUnaryOperationNot)
            {
                res = !res;
            }

            if (_useFixedResult)
            {
                _fixedResult = res;
            }

            return res;
        }

        public override void Reset()
        {
            _fixedResult = null;
        }


        protected virtual bool Inner_ConditionCheck()
        {
            return false;
        }
    }
}