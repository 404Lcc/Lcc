using System.Xml;

namespace LccHotfix
{
    public static partial class NodeConfigTypeRegistry
    {
        static bool _AwaysTrueCndCfg = Register(typeof(AwaysTrueCndCfg), NodeCategory.Cnd);
    }

    public class AwaysTrueCndCfg : ConditionBaseCfg
    {
        public override System.Type NodeType()
        {
            return typeof(AwaysTrueCnd);
        }

        public override bool ParseFromXml(XmlNode cndNode)
        {
            return base.ParseFromXml(cndNode);
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