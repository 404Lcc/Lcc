namespace LccHotfix
{
    //静态配置
    public class RandomChanceCndCfg : ConditionBaseCfg
    {
        public float ProbPercent { get; protected set; } = 0f; //百分比概率

        public override System.Type NodeType()
        {
            return typeof(RandomChanceCnd);
        }
    }

    /// <summary>
    /// 随机概率条件
    /// </summary>
    public class RandomChanceCnd : ConditionNodeBase
    {
        private RandomChanceCndCfg _cfg;
        private float _randNum;

        public override void InitializeNode(ICustomNodeCfg cfg, in CustomNodeContext context)
        {
            base.InitializeNode(cfg, context);
            _cfg = cfg as RandomChanceCndCfg;
            _randNum = UnityEngine.Random.Range(0f, 100f);
        }

        public override void Destroy()
        {
            _randNum = 0f;
            _cfg = null;
            base.Destroy();
        }

        protected override bool Inner_ConditionCheck()
        {
            return _randNum < _cfg.ProbPercent;
        }
    }
}