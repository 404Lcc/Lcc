using System.Xml;

namespace LccHotfix
{
    public static partial class NodeConfigTypeRegistry
    {
        static bool _LogBhvCfg = Register(typeof(LogBhvCfg), NodeCategory.Bhv);
    }

    public class LogBhvCfg : ICustomNodeCfg, IParseFromXml
    {
        public string LogStr;

        public System.Type NodeType()
        {
            return typeof(LogBhv);
        }

        public LogBhvCfg()
        {
            LogStr = "";
        }

        public LogBhvCfg(string str)
        {
            LogStr = str;
        }

        public bool ParseFromXml(XmlNode xmlNode)
        {
            var str = XmlHelper.GetAttribute(xmlNode, "LogStr");
            CLHelper.Assert(!string.IsNullOrEmpty(str));
            LogStr = str;
            return true;
        }
    }

    /// <summary>
    /// 运行时节点:  打印Log
    /// </summary>
    public class LogBhv : BehaviorNodeBase
    {
        private string m_LogStr;

        public override void InitializeNode(ICustomNodeCfg cfg, in CustomNodeContext context)
        {
            base.InitializeNode(cfg, context);
            var theCfg = cfg as LogBhvCfg;
            CLHelper.Assert(theCfg != null);
            m_LogStr = theCfg.LogStr;
        }

        public override void Destroy()
        {
            m_LogStr = null;
            base.Destroy();
        }

        protected override void OnBegin()
        {
            if (m_LogStr == null)
                return;
            CLHelper.LogInfo(this, m_LogStr);
        }
    }
}