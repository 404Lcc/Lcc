namespace LccHotfix
{
    /// <summary>
    /// 节点类型
    /// </summary>
    public enum NodeType
    {
        WINDOW = 0, //窗口
        ROOT = 1, //根节点
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
        AUTO = 0, //延时自动销毁
        CHANGE_PROCEDURE = 1, //切换流程
        DEEPLY = 2, //深度清理
        NEVER = 3, //永久缓存
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