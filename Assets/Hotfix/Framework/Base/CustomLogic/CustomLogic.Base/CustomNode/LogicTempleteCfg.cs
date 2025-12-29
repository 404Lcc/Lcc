using System.Xml;

namespace LccHotfix
{
    public static partial class NodeConfigTypeRegistry
    {
        static bool _LogicTempleteNodeCfg = Register(typeof(LogicTempleteCfg), NodeCategory.Mixture);
    }

    /// <summary>
    /// CustomLogic模板节点， 等价于在该LogicTempletNode处直接插入TempletLogic的全部节点
    /// </summary>
    public class LogicTempleteCfg : ICustomNodeCfg, IParseFromXml
    {
        public int LogicID { get; protected set; }

        public System.Type NodeType()
        {
            LogWrapper.LogError("ERROR : try to Initialize LogicTempleteNode!");
            return null;
        }

        public LogicTempleteCfg()
        {
        }

        public LogicTempleteCfg(int logicID)
        {
            LogicID = logicID;
        }

        public bool ParseFromXml(XmlNode xmlNode)
        {
            string str = XmlHelper.GetAttribute(xmlNode, "LogicID");
            CLHelper.Assert(!string.IsNullOrEmpty(str));
            LogicID = XmlHelper.GetInt(xmlNode, "LoopCnt", -1);
            if (LogicID == -1)
            {
                return false;
            }

            return true;
        }
    }
}