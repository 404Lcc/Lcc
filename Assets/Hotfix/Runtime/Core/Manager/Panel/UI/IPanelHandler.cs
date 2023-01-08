namespace LccHotfix
{
    public interface IPanelHandler
    {
        /// <summary>
        /// 初始化UI组件
        /// </summary>
        /// <param name="panel"></param>
        void OnInitComponent(Panel panel);
        /// <summary>
        /// 初始化数据
        /// </summary>
        /// <param name="panel"></param>
        void OnInitData(Panel panel);

        /// <summary>
        /// 注册UI业务逻辑事件
        /// </summary>
        /// <param name="panel"></param>
        void OnRegisterUIEvent(Panel panel);

        /// <summary>
        /// 显示UI界面
        /// </summary>
        /// <param name="panel"></param>
        /// <param name="contextData"></param>
        void OnShow(Panel panel, AObjectBase contextData = null);

        /// <summary>
        /// 隐藏UI界面
        /// </summary>
        /// <param name="panel"></param>
        void OnHide(Panel panel);

        /// <summary>
        /// 重置界面
        /// </summary>
        /// <param name="panel"></param>
        void OnReset(Panel panel);

        /// <summary>
        /// 销毁界面之前
        /// </summary>
        /// <param name="panel"></param>
        void OnBeforeUnload(Panel panel);

        /// <summary>
        /// 判断是否返回
        /// </summary>
        /// <param name="panel"></param>
        /// <returns></returns>
        bool IsReturn(Panel panel);
    }
}