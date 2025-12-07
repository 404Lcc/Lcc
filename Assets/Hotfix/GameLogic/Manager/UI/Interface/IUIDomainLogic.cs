namespace LccHotfix
{
    public interface IUIDomainLogic : IUILogic
    {
        /// <summary>
        /// 增加子节点
        /// </summary>
        void OnAddChildNode(ElementNode node);

        /// <summary>
        /// 移除子节点
        /// </summary>
        void OnRemoveChildNode(ElementNode node);

        /// <summary>
        /// 子节点请求退出
        /// </summary>
        /// <returns></returns>
        bool OnRequireEscape(ElementNode node);
    }
}