namespace LccHotfix
{
    public class AwaysTrueCndCfg : ConditionBaseCfg
    {
        public override System.Type NodeType()
        {
            return typeof(AwaysTrueCnd);
        }
    }

    public class AwaysTrueCnd : ConditionNodeBase
    {
        protected override bool Inner_ConditionCheck()
        {
            return true;
        }
    }
}