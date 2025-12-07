using System.Collections.Generic;
using System.Linq;
using LccHotfix;
using UnityEngine;

public enum UILayerID
{
    HUD,
    Debug
}

public static class RectTransformEx
{
    public static void AttachToParent(this RectTransform transform, Transform parent)
    {
        // Attached to (0, 0), Scale(1.0, 1.0, 1.0)
        transform.SetParent(parent, false);
        transform.localPosition = Vector3.zero;
        transform.localRotation = Quaternion.identity;
        transform.localScale = Vector3.one;

        // Full adaptive, no padding
        transform.anchoredPosition = Vector2.zero;
        transform.anchorMin = Vector2.zero;
        transform.anchorMax = Vector2.one;
        transform.sizeDelta = Vector2.zero;
    }
}

public class UILayer
{
    private const int LayerStep = 2048;
    private const int OrderStep = 16;

    public UILayerID LayerID { get; }
    public List<ElementNode> UIPanels { get; } = new();

    private bool _bIsVisible = false;
    private bool _bCovered = false;

    private readonly UIRoot _uiRoot;
    private GameObject _root;
    private Transform _trans;
    private CanvasGroup _canvasGroup;


    public UILayer( UIRoot uiRoot, UILayerID layerID)
    {
        _uiRoot = uiRoot;
        LayerID = layerID;
    }

    public void Create(Transform rootTransform)
    {
        var go = new GameObject("Layer_" + LayerID.ToString())
        {
            // layer = LayerID == UILayerID.HUD ? UIConstant.LayerMaskHUD : UIConstant.LayerMaskUI
        };
        var trans = go.AddComponent<RectTransform>();
        trans.AttachToParent(rootTransform);

        _root = go;
        _trans = trans;
        _canvasGroup = go.AddComponent<CanvasGroup>();
        SetActive(true);
    }

    public void Destroy()
    {
        foreach (var panel in UIPanels)
        {
            GameObject.Destroy(panel.GameObject);
        }

        SetActive(false);
        Object.Destroy(_root);
        _root = null;
        _trans = null;
        _canvasGroup = null;
    }

    public void AttachPanel(ElementNode panel)
    {
        var sortingOrder = 0 == UIPanels.Count ? LayerStep * (int)LayerID : UIPanels.Last().SortingOrder + OrderStep;
        panel.SortingOrder = sortingOrder;
        UIPanels.Add(panel);
        UIPanels.Sort((l, r) => l.SortingOrder - r.SortingOrder);
    }

    public void AttachPanelWidget(ElementNode panel)
    {
        // panel.UserWidget.SetLayerRecursive(LayerID == UILayerID.HUD ? UIConstant.LayerMaskHUD : UIConstant.LayerMaskUI);

        var trans = panel.GameObject.GetComponent<RectTransform>();
        trans.AttachToParent(_trans);
        trans.pivot = new Vector2(0.5f, 0.5f);

        panel.Canvas.overrideSorting = true;
        var sortingOrder = panel.SortingOrder;
        var childCanvases = panel.GameObject.GetComponentsInChildren<Canvas>();
        for (int i = 0; i < childCanvases.Length; i++)
        {
            childCanvases[i].sortingOrder += sortingOrder;
        }

        if (panel.IsFullScreen)
        {
            // _uiRoot?.PanelEnterFullscreen(panel);
        }
    }

    public void DetachPanel(ElementNode panel)
    {
        panel.SortingOrder = 0;
        UIPanels.Remove(panel);
    }

    public void DetachPanelWidget(ElementNode panel)
    {
        if (panel.IsFullScreen)
        {
            // _uiRoot?.PanelLeaveFullscreen(panel);
        }

        panel.Canvas.overrideSorting = false;
        panel.GameObject.transform.SetParent(null);
    }

    public void SetActive(bool bVisible)
    {
        bVisible = bVisible && !_bCovered;
        if (_bIsVisible == bVisible)
            return;

        _bIsVisible = bVisible;

        if (!_root)
            return;

        _canvasGroup.alpha = bVisible ? 1 : 0;
        _canvasGroup.blocksRaycasts = bVisible;
        _canvasGroup.interactable = bVisible;
    }

    public bool SetCovered(bool bCovered)
    {
        if (_bCovered == bCovered)
            return false;

        _bCovered = bCovered;
        SetActive(!bCovered);

        foreach (var panel in UIPanels)
        {
            // panel?.SetCoveredByLayer(bCovered);
        }

        return true;
    }

    public bool ChangeCoverStateBelowOrder(int order, bool bCovered)
    {
        for (int index = UIPanels.Count - 1; index >= 0; --index)
        {
            var panel = UIPanels[index];
            // var bIsBelowOrder = panel?.SortingOrder < order;
            // if (bIsBelowOrder && panel.SetCovered(bCovered))
            //     return true;
        }

        return false;
    }
}