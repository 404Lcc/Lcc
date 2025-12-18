using UnityEngine;

namespace LccHotfix
{
    /// <summary>
    /// UI渲染的根元素，概念上等于一张逻辑画布
    /// </summary>
    public interface IUIRoot
    {
        /// <summary>
        /// 渲染UIRoot画布的相机
        /// </summary>
        public Camera RenderCamera { get; }

        /// <summary>
        /// 渲染UIRoot用的画布
        /// </summary>
        public Canvas Canvas { get; }

        /// <summary>
        /// UIRoot的布局
        /// </summary>
        public Transform Transform { get; }

        /// <summary>
        /// 初始化一张画布，进入可以渲染元素的状态
        /// </summary>
        public void Initialize();

        /// <summary>
        /// 销毁一张画布，取消其上所有元素的渲染状态，在再次Initialize之前不可再渲染元素
        /// </summary>
        public void Finalize();

        /// <summary>
        /// 寻找在画布上渲染的元素
        /// </summary>
        /// <param name="name">元素名称</param>
        /// <returns>根据name查到的元素对象</returns>
        public ElementNode Find(string name);

        /// <summary>
        /// 查看元素是否在画布上渲染
        /// </summary>
        /// <param name="elementNode">渲染的元素</param>
        /// <param name="name">渲染的元素名称</param>
        /// <returns></returns>
        public bool Find(ElementNode elementNode, out string name);

        /// <summary>
        /// 将元素渲染到画布上
        /// </summary>
        /// <param name="name">元素名称</param>
        /// <param name="elementNode">需要渲染的元素</param>
        public void Attach(string name, ElementNode elementNode);

        /// <summary>
        /// 将元素从画布上摘除
        /// </summary>
        public void Detach(ElementNode elementNode);

        public UILayer GetLayerByID(UILayerID layerID);

    }
}