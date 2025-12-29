using System.Xml;

namespace LccHotfix
{
    /// <summary>
    /// CustomLogic相关一些调试、辅助代码
    /// </summary>
    public static class CLHelper
    {
        public static void AssertBreak()
        {
            LogWrapper.LogError("KaHotUpdate.CoreGameLogic Has ERROR! ");
            UnityEngine.Debug.Break();
        }

        public static bool Assert(bool condition, object logMsg = null)
        {
            if (condition)
                return true;
            if (logMsg != null)
            {
                LogWrapper.LogError(logMsg.ToString());
            }

            AssertBreak();
            return false;
        }

        /// Node Helper
        public static void LogError(this CustomNode node, string logMsg)
        {
            int id = node.GenInfo.LogicConfigID;
            LogWrapper.LogError($"LogicNodeError id={id} : {logMsg}");
        }

        public static void LogInfo(this CustomNode node, string logMsg)
        {
            int id = node.GenInfo.LogicConfigID;
            LogWrapper.LogInfo($"Logic[ {id} ]({node.CreationIndex}): {logMsg}");
        }

        public static bool IsNodeCanStop(this CustomNode node)
        {
            if (node != null && node is INeedStopCheck check)
            {
                return check.CanStop();
            }

            return true;
        }

        public static void AssertNodeCfgCategory(ICustomNodeCfg nodeCfg, NodeCategory targetCategory, bool checkNull = true)
        {
            if (nodeCfg != null)
            {
                var category = NodeConfigTypeRegistry.GetNodeCfgCategory(nodeCfg.GetType());
                CLHelper.Assert(category == targetCategory);
            }
            else if (checkNull)
            {
                LogWrapper.LogError("LogicError nodeCfg == null");
            }
        }



        /// XmlNode Helper
        public static bool Assert(XmlNode cfgNode, bool condition, string logMsg = null)
        {
            if (condition)
                return true;
            LogError(cfgNode, logMsg);
            return false;
        }

        public static void LogError(XmlNode cfgNode, string logMsg)
        {
            int id = cfgNode.GetSingleNodeID();
            LogWrapper.LogError($"CLHelper XmlNode ParseError id={id} : {logMsg}");
        }

        public static int GetSingleNodeID(this XmlNode cfgNode)
        {
            XmlNode node = cfgNode;
            while (node != null)
            {
                XmlNode idnode = node.SelectSingleNode("ID");
                if (idnode != null)
                {
                    int id = -1;
                    int.TryParse(idnode.InnerText, out id);
                    return id;
                }

                node = node.ParentNode;
            }

            LogWrapper.LogError("XmlNode GetSingleNodeID ERROR!");
            return -1;
        }

        public static ICustomNodeCfg CreateNodeCfg(XmlNode node)
        {
            if (node == null)
            {
                return null;
            }

            XmlElement cusNode = node as XmlElement;
            if (cusNode == null)
            {
                CLHelper.Assert(false, "CustomLogicConfig PraseNodeCfg ParseError  cusNode as XmlElement == null");
                return null;
            }

            string nodeTypeStr = string.Format("{0}{1}", cusNode.GetAttribute("type"), "Cfg");
            ICustomNodeCfg nodeCfg = NodeConfigTypeRegistry.CreateCustomNodeCfg(nodeTypeStr);
            if (nodeCfg == null)
            {
                CLHelper.Assert(false, "NodeConfigTypeRegistry.CreateCustomNodeCfg == null  nodeTypeStr = " + nodeTypeStr);
                return null;
            }

            var xmlNodeCfg = nodeCfg as IParseFromXml;
            if (xmlNodeCfg != null)
            {
                if (!xmlNodeCfg.ParseFromXml(node))
                {
                    CLHelper.LogError(node, nodeTypeStr);
                }
            }

            CLHelper.Assert(nodeCfg != null);
            return nodeCfg;
        }

    }
}