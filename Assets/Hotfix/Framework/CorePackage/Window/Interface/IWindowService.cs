using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using LccHotfix;
using UnityEngine;

public interface IWindowService : IService
{
    #region Res

    /// <summary>
    /// 同步加载GameObject
    /// </summary>
    Action<AssetLoader, string, Action<GameObject>> LoadGameObject { get; set; }

    #endregion

    #region Types

    //初始化获取logic类
    void InitializeForAssembly(Assembly assembly);

    //根据窗口，创建logic
    void CreateUILogic(Window window);

    IUILogic CreateLogic(string logicName, Window window);

    #endregion

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

    WRootNode CommonRoot { get; }

    //初始化通用节点
    void Init();

    //根据一个窗口打开一个新窗口
    void OpenWindow(WNode openBy, string windowName, object[] param);

    /// <summary>
    /// 打开一个界面
    /// 这里只是创建，并不会改变当前栈结构
    /// 确认界面可打开后才会继续
    /// </summary>
    /// <param name="windowName"></param>
    /// <param name="param"></param>
    /// <returns></returns>
    void OpenWindow(string windowName, object[] param);

    //打开根节点
    WRootNode OpenRoot(string rootName, object[] param);

    /// <summary>
    /// 关闭一个窗口
    /// window是有作用域的
    /// 通过这个方法默认是关闭一个栈内的全屏窗口或当前栈顶窗口的子窗口
    /// 不能用来关闭一个不活跃窗口的子窗口
    /// </summary>
    /// <param name="windowClose"></param>
    /// <returns></returns>
    object CloseWindow(string windowClose);

    //栈顶节点关闭互斥子节点
    void CloseWindow(int windowFlag);

    //关闭全部窗口
    void CloseAllWindow();

    //返回键请求关闭窗口处理
    void EscapeTopWindow();


    /// <summary>
    /// 关闭root时从栈内移除
    /// </summary>
    /// <param name="root"></param>
    void RemoveRoot(WRootNode root);

    //增加到释放队列
    void AddToReleaseQueue(WNode node);

    /// <summary>
    /// 释放全部window资源
    /// </summary>
    /// <param name="type">筛选释放window的级别，释放小于这个级别的所有窗口</param>
    void ReleaseAllWindow(ReleaseType level = ReleaseType.AUTO);

    //增加节点关闭回调
    void AddCloseCallback(string windowName, Action<object> callback);

    //移除节点关闭回调
    void RemoveCloseCallback(string windowName, Action<object> callback);

    //触发节点关闭回调
    void OnWindowClose(string windowName, object backValue);

    //获取窗口，根据窗口名称
    Window GetWindow(string windowName);

    //获取根节点，根据根节点名称
    WRootNode GetRoot(string rootName);

    //获取栈顶的根节点
    WRootNode GetTopRoot();

    //获取栈顶的最新窗口
    Window GetTopWindow();

    /// <summary>
    /// 显示一个碰撞框，不能再次点击
    /// </summary>
    /// <param name="maskType"></param>
    void ShowMaskBox(int maskType, bool enable);

    /// <summary>
    /// 屏幕遮黑淡入
    /// 替换以前的截屏操作，这个更快，不需要等待一帧
    /// </summary>
    void ShowScreenMask();
}