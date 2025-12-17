using System.Collections.Generic;
using System.Linq;
using LccHotfix;
using UnityEngine;

public enum UILayerID
{
    HUD,
    Main,
    Popup,
    Debug
}

public class UILayer
{
    private const int LayerStep = 2048;
    private const int OrderStep = 16;

    private UIRoot _uiRoot;
    private GameObject _layer;
    private RectTransform _rectTransform;
    private CanvasGroup _canvasGroup;
    
    public UILayerID UILayerID { get; }
    public List<ElementNode> UIElementList { get; } = new List<ElementNode>();

    public UILayer(UIRoot uiRoot, UILayerID layerID)
    {
        _uiRoot = uiRoot;
        UILayerID = layerID;
    }

    public void Create(Transform canvasTransform)
    {
        var go = new GameObject("Layer_" + UILayerID)
        {
            layer = UIConstant.LayerMaskUI
        };
        var rect = go.AddComponent<RectTransform>();
        AttachToParent(rect, canvasTransform);

        _layer = go;
        _rectTransform = rect;
        _canvasGroup = go.AddComponent<CanvasGroup>();
    }

    public void Destroy()
    {
        foreach (var item in UIElementList)
        {
            GameObject.Destroy(item.GameObject);
        }

        Object.Destroy(_layer);
        _layer = null;
        _rectTransform = null;
        _canvasGroup = null;
    }

    public void AttachElement(ElementNode elementNode)
    {
        var sortingOrder = 0 == UIElementList.Count ? LayerStep * (int)UILayerID : UIElementList.Last().SortingOrder + OrderStep;
        elementNode.SetSortingOrder(sortingOrder);
        UIElementList.Add(elementNode);
        UIElementList.Sort((l, r) => l.SortingOrder - r.SortingOrder);
    }

    public void AttachElementWidget(ElementNode elementNode)
    {
        var rect = elementNode.RectTransform;
        AttachToParent(rect, _rectTransform);
        rect.pivot = new Vector2(0.5f, 0.5f);

        elementNode.Canvas.overrideSorting = true;
        var sortingOrder = elementNode.SortingOrder;
        var childCanvases = elementNode.GameObject.GetComponentsInChildren<Canvas>();
        for (int i = 0; i < childCanvases.Length; i++)
        {
            childCanvases[i].sortingOrder += sortingOrder;
        }
    }

    public void DetachElementWidget(ElementNode elementNode)
    {
        var sortingOrder = elementNode.SortingOrder;
        var childCanvases = elementNode.GameObject.GetComponentsInChildren<Canvas>();

        for (int i = 0; i < childCanvases.Length; i++)
        {
            childCanvases[i].sortingOrder -= sortingOrder;
        }

        elementNode.Canvas.overrideSorting = false;
    }
    
    public void DetachElement(ElementNode elementNode)
    {
        elementNode.SetSortingOrder(0);
        UIElementList.Remove(elementNode);
    }

    public void AttachToParent(RectTransform rect, Transform parent)
    {
        rect.SetParent(parent, false);
        rect.localPosition = Vector3.zero;
        rect.localRotation = Quaternion.identity;
        rect.localScale = Vector3.one;

        rect.anchoredPosition = Vector2.zero;
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.sizeDelta = Vector2.zero;
    }
}