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
        if (Model.LoadSceneManager.Instance.process >= 100)
        {
            OnHidePanel();
        }
    }
    public void InitPanel()
    {
    }
    public void OnHidePanel()
    {
        PanelManager.Instance.ClearPanel(PanelType.Load);
    }
}