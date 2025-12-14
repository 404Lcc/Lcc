using System;
using LccHotfix;
using UnityEngine;

public interface IWindowService : IService
{
    IUIRoot Root { get; }
    Transform UIRoot { get; set; }
    Camera UICamera { get; set; }
    Action<AssetLoader, string, Action<GameObject>> LoadAsyncGameObject { get; set; }
    void Init();
    IUILogic GetUILogic(string name, UINode node);

    void ShowDomain(string domainName, string elementName, params object[] args);
    void ShowDomain(string name, params object[] args);
    void ShowElement(string name, params object[] args);

    object HideElement(string name);
    void HideTopNode();
    void HideAllDomain();
    
    DomainNode GetDomain(string name);
    T GetDomain<T>(string name) where T : UIDomainBase;
    ElementNode GetElement(string name);
    T GetElement<T>(string name) where T : UIElementBase;
    DomainNode GetTopDomain();
    ElementNode GetTopElement();

    bool IsElementActive(string name);

    void RemoveDomainFromStack(DomainNode node);
    void AddToReleaseQueue(UINode node);
    void ForceClearReleaseQueue(ReleaseType level = ReleaseType.Auto);

    void AddNodeHideCallback(string name, Action<object> callback);
    void RemoveNodeHideCallback(string name, Action<object> callback);
    void DispatchNodeHide(string name, object returnValue);
}