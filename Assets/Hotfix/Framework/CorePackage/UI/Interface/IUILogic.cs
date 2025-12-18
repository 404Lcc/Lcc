using System;

namespace LccHotfix
{
    public interface IUILogic
    {
        /// <summary>
        /// 访问所属node
        /// </summary>
        UINode Node { get; set; }
        
        /// <summary>
        /// 回退类型
        /// </summary>
        public EscapeType EscapeType { get; }

        /// <summary>
        /// 释放类型
        /// </summary>
        public ReleaseType ReleaseType { get; }

        /// <summary>
        /// 初始化参数
        /// </summary>
        void OnConstruct();
        
        /// <summary>
        /// 创建
        /// </summary>
        void OnCreate();

        /// <summary>
        /// 打开前的准备，可以在这里请求数据
        /// </summary>
        void OnSwitch(Action<bool> callback);

        /// <summary>
        /// 覆盖
        /// </summary>
        /// <param name="covered"></param>
        void OnCovered(bool covered);

        /// <summary>
        /// 显示
        /// </summary>
        /// <param name="paramsList"></param>
        void OnShow(object[] paramsList);

        /// <summary>
        /// 重新显示界面
        /// </summary>
        /// <param name="paramsList"></param>
        void OnReShow(object[] paramsList);

        /// <summary>
        /// 更新
        /// </summary>
        void OnUpdate();

        /// <summary>
        /// 隐藏
        /// </summary>
        object OnHide();

        /// <summary>
        /// 删除
        /// </summary>
        void OnDestroy();

        /// <summary>
        /// Escape退出结果
        /// </summary>
        /// <returns></returns>
        bool OnEscape(ref EscapeType escapeType);
    }
}