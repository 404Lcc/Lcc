namespace LccHotfix
{
    /// <summary>
    /// 节点类型
    /// </summary>
    public enum NodeType
    {
        WINDOW = 0,//窗口
        ROOT = 1,//根节点
    }

    /// <summary>
    /// 节点状态、阶段
    /// </summary>
    public enum NodePhase
    {
        DEACTIVE,  //未激活
        OPENED,    //已打开，但不显示
        ACTIVE     //激活显示
    }

    /// <summary>
    /// 释放方式
    /// </summary>
    public enum ReleaseType
    {
        IMMEDIATE = -1,//立即销毁
        AUTO = 0,//延时自动销毁
        CHANGE_PROCEDURE = 1,//切换流程
        DEEPLY = 2,//深度清理
        NEVER = 3,//永久缓存
    }

    /// <summary>
    /// esc响应方式
    /// </summary>
    public enum EscapeType
    {
        SKIP_OVER = 0,    //跳过
        AUTO_CLOSE = 1,    //关闭
        REFUSE_AND_BREAK = 2,	//中断
    }

    /// <summary>
    /// 节点特点标记
    /// </summary>
    public enum NodeFlag
    {
        NONE = 0,
        FULL_SCREEN = 1 << 0,   // 全屏界面
        MAIN_NODE = 1 << 1,   // 主要节点，当root的所有主节点被关闭时，root会被关闭
        TOP_NODE = 1 << 2,   // 顶层节点，不会被全屏遮挡
    }

    /// <summary>
    /// 互斥标志
    /// </summary>
    public enum RejectFlag : long
    {
        NONE = 0,
        MAIN = 1 << 0,
    }

    public enum MaskType
    {
        WINDOW_SWITCH = 1 << 0,    // 切换窗口的过程
        WINDOW_ANIM = 1 << 1,
        CHANGE_PROCEDURE = 1 << 2,
        NET_REQUEST = 1 << 3,
    }
}