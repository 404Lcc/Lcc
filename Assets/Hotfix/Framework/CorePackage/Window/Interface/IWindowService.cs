using System;
using System.Reflection;
using LccHotfix;
using UnityEngine;

public interface IWindowService : IService
{
    IUIRoot Root { get;  }
    Transform WindowRoot { get; set; }
    Camera UICamera { get; set; }
    Action<AssetLoader, string, Action<GameObject>> LoadAsyncGameObject { get; set; }
    void Init();

    IUILogic CreateLogic(string logicName, UINode node);
    
    void OpenWindow(string windowName, object[] param);

    void ShowWindow(string windowName, string rootName, object[] param);

    void ShowRoot(string rootName, object[] param);

    /// <summary>
    /// 关闭一个窗口，通过这个方法默认是关闭一个栈内的一个界面（不能用来关闭一个不活跃窗口的子窗口）
    /// </summary>
    /// <param name="windowClose"></param>
    /// <returns></returns>
    object HideWindow(string windowClose);

    //关闭全部窗口
    void HideAllWindow();

    //返回键请求关闭窗口处理
    void EscapeTopWindow();

    /// <summary>
    /// 关闭root时从栈内移除
    /// </summary>
    /// <param name="root"></param>
    void RemoveRoot(DomainNode root);

    //增加到释放队列
    void AddToReleaseQueue(UINode node);

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
    ElementNode GetWindow(string windowName);

    //获取根节点，根据根节点名称
    DomainNode GetRoot(string rootName);

    //获取栈顶的根节点
    DomainNode GetTopRoot();

    //获取栈顶的最新窗口
    ElementNode GetTopWindow();
}