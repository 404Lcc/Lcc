using System;
using System.Collections;
using UnityEngine;

namespace LccHotfix
{
    public interface IUIService : IService
    {
        Camera UICamera { get; }

        void SetUIHelper(IUIHelper uiHelper);

        void Init();

        IUILogic GetMonoLogic(Window window, Type monoType);

        WindowMode GetWindowMode(string windowName);

        bool IsEscapeEnable();


        #region 打开面板

        UILogicBase OpenPanel(string panelID, params object[] paramsList);

        void BuildPanelDepth(GameObject obj, int depth);


        /// <summary>
        /// 延迟打开界面
        /// </summary>
        /// <param name="panelName"></param>
        /// <param name="time"></param>
        /// <param name="onFinish"></param>
        /// <param name="args"></param>
        /// <returns></returns>
        IEnumerator OpenPanelDelay(string panelName, float time, Action onFinish, params object[] args);

        #endregion


        #region 关闭面板

        void ClosePanel(int rejectFlag);

        object ClosePanel(string panelID);


        /// <summary>
        /// 顶部栏和返回键调用，关闭当前ui
        /// </summary>
        /// <param name="withCloseApp">最后一个ui时调用，是否弹关闭游戏提示</param>
        void CloseTopPanel(bool withCloseApp);

        #endregion

        #region 查询方法

        UILogicBase GetPanel(string panelID);

        IUILogic GetPanelLogic(string panelID);

        bool IsPanelActive(string panelID);


        string GetTopPanelID();

        UILogicBase GetTopPanel();

        #endregion


        #region UI辅助管理方法

        /// <summary>
        /// 显示一个全屏遮罩，用于业务处理
        /// </summary>
        /// <param name="state"></param>
        void ShowMask(int maskType, bool enable);

        bool GetMaskState();

        #endregion

        #region Turn

        /// <summary>
        /// 跳转至指定UI，校验功能是否开启
        /// </summary>
        /// <param name="gotoID"></param>
        /// <param name="args"></param>
        /// <returns></returns>
        bool JumpToPanelByID(int gotoID, params object[] args);


        /// <summary>
        /// 有些面板不能直接打开，打开方式比较特殊
        /// </summary>
        /// <param name="turnNode"></param>
        /// <returns></returns>
        bool OpenSpecialPanel(WNode.TurnNode turnNode);

        #endregion

        #region Popup

        /// <summary>
        /// 保存弹窗数据，在主城会按顺序弹出 最多支持三个参数
        /// </summary>
        /// <param name="type">面板类型</param>
        /// <param name="param">面板参数</param>
        void StackPanel(string panelName, object[] param);

        void StackPanel(string panelName);

        void StackPanel(string panelName, object param);

        /// <summary>
        /// 判断是否压入了某种类型的弹窗
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        bool IsStackPanel(string panelName);

        /// <summary>
        /// 打开弹窗
        /// </summary>
        void TryPopupPanel();


        void OnPopPanelClose(string panelName);

        #endregion
    }
}