using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using LccHotfix;
using UnityEngine;

public interface IWindowService : IService
{
    /// <summary>
    /// 获取窗口的父节点
    /// </summary>
    Transform WindowRoot { get; set; }

    /// <summary>
    /// ui相机
    /// </summary>
    Camera UICamera { get; set; }

    #region Res

    /// <summary>
    /// 同步加载GameObject
    /// </summary>
    Action<AssetLoader, string, Action<GameObject>> LoadAsyncGameObject { get; set; }

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
    /// 从表里获取窗口的配置
    /// </summary>
    Func<string, WindowMode> GetModeFunc { get; set; }

    #endregion

    WRootNode CommonRoot { get; }

    //初始化通用节点
    void Init();

    UILayer GetUILayer(UILayerID layerID);

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
}