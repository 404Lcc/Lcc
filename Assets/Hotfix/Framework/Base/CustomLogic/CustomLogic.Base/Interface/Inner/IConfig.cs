using System.Collections.Generic;

namespace LccHotfix
{
    /// <summary>
    /// 静态配置接口
    /// </summary>
    public interface INodeCfgList
    {
        List<ICustomNodeCfg> NodeCfgList { get; }
    }

    /// <summary>
    /// 可由Xml节点初始化自身数据
    /// </summary>
    public interface IParseFromXml
    {
        bool ParseFromXml(System.Xml.XmlNode node);
    }

    /// <summary>
    /// 含有子节点配置 (静态分析配置用，一些需要递归遍历收集信息的进阶需求会用到，比如寻找可能会使用的所有资源)
    /// </summary>
    public interface IHasSubNodeCfgList
    {
        List<ICustomNodeCfg> GetNodeCfgList();
    }
}