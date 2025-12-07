using UnityEngine;

namespace LccHotfix
{
    /// <summary>
    /// UI渲染的根节点，概念上等于一张逻辑画布。实现应保证：
    /// - Initialize()调用前不可其上渲染IUIElement对象
    /// - Finalize()调用时取消其上的所有IUIElement对象的渲染状态，调用后不可在其上渲染IUIElement
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
        /// 初始化一张画布，进入可以渲染IUIElement的状态
        /// </summary>
        public void Initialize();

        /// <summary>
        /// 销毁一张画布，取消其上所有IUIElement的渲染状态，在再次Create()之前不可再渲染IUIElement
        /// </summary>
        public void Finalize();

        /// <summary>
        /// 寻找在画布上渲染的元素
        /// </summary>
        /// <param name="elementKey">索引IUIElement用的key</param>
        /// <returns>根据elementKey查到的IUIElement对象</returns>
        public UINode Find(string elementKey);

        /// <summary>
        /// 查看元素是否在画布上渲染，如果是返回对应的elementKey
        /// </summary>
        /// <param name="element">渲染的元素</param>
        /// <param name="elementKey">渲染元素的key</param>
        /// <returns></returns>
        public bool Find(UINode element, out string elementKey);

        /// <summary>
        /// 将IUIElement渲染到画布上，并返回一个Key
        /// </summary>
        /// <param name="elementKey">元素的Key</param>
        /// <param name="element">需要渲染的IUIElement</param>
        /// <returns>elementKey</returns>
        public void Attach(string elementKey, UINode element);

        /// <summary>
        /// 将IUIElement从画布上摘除
        /// </summary>
        public void Detach(UINode element);

        /// <summary>
        /// 将IUIElement从画布上摘除
        /// </summary>
        public UINode DetachByKey(string elementKey);
    }
}