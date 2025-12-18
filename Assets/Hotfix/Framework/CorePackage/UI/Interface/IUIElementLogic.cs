namespace LccHotfix
{
    public interface IUIElementLogic : IUILogic
    {
        /// <summary>
        /// 层级ID
        /// </summary>
        public UILayerID LayerID { get; }

        /// <summary>
        /// 是否全屏
        /// </summary>
        public bool IsFullScreen { get; }

        /// <summary>
        /// 返回节点类型
        /// </summary>
        public NodeType ReturnNodeType { get; }

        /// <summary>
        /// 返回节点名称
        /// </summary>
        public string ReturnNodeName { get; }

        /// <summary>
        /// 返回节点参数
        /// </summary>
        public int ReturnNodeParam { get; }
    }
}