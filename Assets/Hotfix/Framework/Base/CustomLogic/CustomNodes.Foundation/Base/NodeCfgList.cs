using System.Collections.Generic;
using System.Xml;

namespace LccHotfix
{
    public class NodeCfgList<T> : List<T> where T : class, ICustomNodeCfg
    {
        public bool ParseFromXml(XmlNode xmlNode, string xmlNodeName = "Node")
        {
            Clear();
            XmlNodeList subNodeList = xmlNode.SelectNodes(xmlNodeName);
            if (subNodeList == null)
                return false;
            foreach (XmlNode subNode in subNodeList)
            {
                var nodeCfg = CLHelper.CreateNodeCfg(subNode) as T;
                CLHelper.Assert(nodeCfg != null);
                this.Add(nodeCfg);
            }

            if (this.Count == 0)
            {
                CLHelper.LogError(xmlNode, "NodeCfgList.ParseFromXml() CfgList.Count == 0");
                CLHelper.AssertBreak();
                return false;
            }

            return true;
        }
    }
}