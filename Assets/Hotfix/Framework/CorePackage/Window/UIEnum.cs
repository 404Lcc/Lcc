namespace LccHotfix
{
    /// <summary>
    /// 节点类型
    /// </summary>
    public enum NodeType
    {
        Element = 0, //UI元素
        Domain = 1, //域
    }

    /// <summary>
    /// 节点状态、阶段
    /// </summary>
    public enum NodePhase
    {
        Create, //创建
        Show //显示
    }

    /// <summary>
    /// 释放方式
    /// </summary>
    public enum ReleaseType
    {
        Auto = 0, //延迟自动销毁
        ChangeProcedure = 1, //切换流程
        Deeply = 2, //深度清理
        Never = 3, //永久缓存
    }

    /// <summary>
    /// esc响应方式
    /// </summary>
    public enum EscapeType
    {
        Skip = 0, //跳过
        Hide = 1, //隐藏
    }
}