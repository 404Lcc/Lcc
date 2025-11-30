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
        DEACTIVE, //未激活
        ACTIVE //激活显示
    }

    /// <summary>
    /// 释放方式
    /// </summary>
    public enum ReleaseType
    {
        IMMEDIATE = -1, //立即销毁
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
        SKIP_OVER = 0, //跳过
        AUTO_CLOSE = 1, //关闭
        REFUSE_AND_BREAK = 2, //中断
    }
}