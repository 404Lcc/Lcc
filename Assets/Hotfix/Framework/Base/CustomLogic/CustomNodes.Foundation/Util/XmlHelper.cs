using System.Xml;

namespace LccHotfix
{
    /// <summary>
    /// Xml解析的一些辅助
    /// </summary>
    public static partial class XmlHelper
    {
        public static NodeCfgList<T> GetNodeList<T>(XmlNode xmlNode, string subNodeName) where T : class, ICustomNodeCfg
        {
            XmlNodeList subNodeList = xmlNode.SelectNodes(subNodeName);
            if (subNodeList == null)
                return null;
            if (subNodeList.Count == 0)
                return null;
            var cfgList = new NodeCfgList<T>();
            foreach (XmlNode subNode in subNodeList)
            {
                T nodeCfg = CLHelper.CreateNodeCfg(subNode) as T;
                CLHelper.Assert(nodeCfg != null);
                cfgList.Add(nodeCfg);
            }

            if (cfgList.Count == 0)
            {
                CLHelper.AssertBreak();
            }

            return cfgList;
        }
    }
}