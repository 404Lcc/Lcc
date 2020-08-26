using Hotfix;
using UnityEngine;

public class LoadPanel : MonoBehaviour
{
    void Awake()
    {
        InitPanel();
    }
    void Start()
    {
    }
    void Update()
    {
        if (Model.IO.loadSceneManager.process >= 100)
        {
            OnHidePanel();
        }
    }
    public void InitPanel()
    {
    }
    public void OnHidePanel()
    {
        IO.panelManager.ClearPanel(PanelType.Load);
    }
}