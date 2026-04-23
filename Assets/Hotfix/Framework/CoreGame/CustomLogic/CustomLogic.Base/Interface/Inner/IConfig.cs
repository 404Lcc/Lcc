using System.Collections.Generic;

namespace LccHotfix
{
    /// <summary>
    /// 含有子节点配置 (静态分析配置用，一些需要递归遍历收集信息的进阶需求会用到，比如寻找可能会使用的所有资源)
    /// </summary>
    public interface IHasSubNodeCfgList
    {
        List<ICustomNodeCfg> GetNodeCfgList();
    }
}