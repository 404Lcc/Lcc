using System;
using System.Collections;
using System.Collections.Generic;
using System.Net.Security;
using System.Reflection;
using System.Security.Cryptography.X509Certificates;
using cfg;
using LccModel;
using Luban;
using RVO;
using SimpleJSON;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Video;

namespace LccHotfix
{
    public interface IWindowService : IService
    {
        Window OpenWindow(WNode openBy, string windowName, object[] param);

        Window OpenWindow(string windowName, object[] param);

        //打开根节点
        WRootNode OpenRoot(string rootName, object[] param);

        void CloseAllWindow();
        void RemoveRoot(WRootNode root);
        void AddToReleaseQueue(WNode node);
        void ReleaseAllWindow(ReleaseType level = ReleaseType.AUTO);
        void OnWindowClose(string windowName, object backValue);
        void ShowMaskBox(int maskType, bool enable);


        #region Action

        /// <summary>
        /// 从prefab上获取mono对象
        /// </summary>
        Func<Window, Type, IUILogic> GetUILogicMonoFunc { get; set; }

        /// <summary>
        /// 从表里获取窗口的配置
        /// </summary>
        Func<string, WindowMode> GetModeFunc { get; set; }

        /// <summary>
        /// 获取窗口的父节点
        /// </summary>
        Transform WindowRoot { get; set; }

        /// <summary>
        /// ui相机
        /// </summary>
        Camera UICamera { get; set; }

        /// <summary>
        /// 深度排序方法
        /// </summary>
        Action<GameObject, int> SortDepthFunc { get; set; }

        /// <summary>
        /// 获取当前返回键是否生效
        /// </summary>
        Func<bool> EscapeJudgeFunc { get; set; }

        /// <summary>
        /// 暂停一个window的方式
        /// </summary>
        Action<Transform, bool> PauseWindowFunc { get; set; }

        /// <summary>
        /// 刷新背景图
        /// </summary>
        Action<Window, string> RefreshBackgroundFunc { get; set; }

        /// <summary>
        /// 播放界面音效
        /// </summary>
        Action<int> PlayWindowSoundFunc { get; set; }

        /// <summary>
        /// 确认弹窗
        /// </summary>
        Action<string, Action> ShowNoticeFunc { get; set; }

        /// <summary>
        /// 选择弹窗
        /// </summary>
        Action<string, Action, Action> ShowSelectFunc { get; set; }

        /// <summary>
        /// 显示屏幕遮罩
        /// </summary>
        /// <returns></returns>
        Action ShowScreenMaskFunc { get; set; }

        /// <summary>
        /// 显示屏幕遮挡碰撞框
        /// </summary>
        /// <returns></returns>
        Action<int, bool> ShowMaskBoxFunc { get; set; }

        /// <summary>
        /// 获取屏幕遮挡状态
        /// </summary>
        Func<bool> GetMaskBoxStateFunc { get; set; }

        /// <summary>
        /// 关闭最后一个root
        /// </summary>
        Action OnClosedLastRootFunc { get; set; }

        #endregion


        #region Turn

        /// <summary>
        /// 跳转至指定UI，校验功能是否开启
        /// </summary>
        /// <param name="gotoID"></param>
        /// <param name="args"></param>
        /// <returns></returns>
        bool JumpToWindowByID(int gotoID, params object[] args);


        /// <summary>
        /// 有些面板不能直接打开，打开方式比较特殊
        /// </summary>
        /// <param name="turnNode"></param>
        /// <returns></returns>
        bool OpenSpecialWindow(WNode.TurnNode turnNode);

        #endregion

        #region Init

        void InitWindowManager();

        WindowMode GetWindowMode(string windowName);

        IUILogic GetUILogic(Window window, Type monoType);

        bool IsEscapeEnable();

        #region 打开面板

        T OpenWindow<T>(string windowName, params object[] paramsList) where T : UILogicBase;


        /// <summary>
        /// 延迟打开界面
        /// </summary>
        /// <param name="windowName"></param>
        /// <param name="time"></param>
        /// <param name="onFinish"></param>
        /// <param name="args"></param>
        /// <returns></returns>
        IEnumerator OpenWindowDelay(string windowName, float time, Action onFinish, params object[] args);

        #endregion


        #region 查询方法

        T GetUILogic<T>(string windowName) where T : UILogicBase;


        bool IsWindowActive(string windowName);

        string GetTopWindowName();
        T GetTopWindow<T>() where T : UILogicBase;

        #endregion


        #region UI辅助管理方法

        /// <summary>
        /// 显示一个全屏遮罩，用于业务处理
        /// </summary>
        /// <param name="state"></param>
        void ShowMask(int maskType, bool enable);

        bool GetMaskState();

        #endregion

        #endregion


        #region Popup

        /// <summary>
        /// 保存弹窗数据，在主城会按顺序弹出 最多支持三个参数
        /// </summary>
        /// <param name="type">面板类型</param>
        /// <param name="param">面板参数</param>
        void StackWindow(string windowName, object[] param);

        void StackWindow(string windowName);

        void StackWindow(string windowName, object param);

        /// <summary>
        /// 判断是否压入了某种类型的弹窗
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        bool IsStackWindow(string windowName);

        /// <summary>
        /// 打开弹窗
        /// </summary>
        void TryPopupWindow();


        void OnPopWindowClose(string windowName);

        #endregion


        #region Res

        /// <summary>
        /// 同步加载GameObject
        /// </summary>
        Func<string, bool, GameObject> LoadGameObject { get; set; }

        #endregion


        #region Types

        //初始化获取logic类
        void InitializeForAssembly(Assembly assembly);

        //根据窗口，创建logic
        void CreateUILogic(Window window);

        //根据logic名称和窗口，创建logic
        IUILogic CreateLogic(string logicName, Window window);

        #endregion
    }
}