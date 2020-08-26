using Model;
using UnityEngine;

public class LaunchPanel : MonoBehaviour
{
    void Start()
    {
        InitPanel();
    }
    public void InitPanel()
    {
        OnHidePanel();
    }
    public void OnHidePanel()
    {
        IO.panelManager.ClearPanel(PanelType.Launch);
    }
}