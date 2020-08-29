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
#if AssetBundle
        IO.panelManager.OpenPanel(PanelType.Updater);
#else
#if ILRuntime
        IO.ilRuntimeManager.InitManager();
#else
        IO.monoManager.InitManager();
#endif
#endif
        OnHidePanel();
    }
    public void OnHidePanel()
    {
        IO.panelManager.ClearPanel(PanelType.Launch);
    }
}