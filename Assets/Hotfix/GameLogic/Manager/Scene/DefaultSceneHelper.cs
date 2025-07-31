using System.Collections;
using LccHotfix;
using LccModel;
using UnityEngine;
using Init = LccHotfix.Init;

public class DefaultSceneHelper : ISceneHelper
{
    public IEnumerator ShowSceneLoading(LoadingType loadType)
    {
        UILoadingPanel loadingPanel = null;
        switch (loadType)
        {
            case LoadingType.Normal:
                loadingPanel = UILoadingPanel.Instance;
                loadingPanel.Show(string.Empty);
                loadingPanel.UpdateLoadingPercent(0, 5);
                yield return null;
                break;
            case LoadingType.Fast:
                UIForeGroundPanel.Instance.FadeOut(1.5f);
                yield return null;
                break;
        }
    }

    public void ResetSpeed()
    {
        Launcher.Instance.SetGameSpeed(1);
        Launcher.Instance.ChangeFPS();
    }
    public void UpdateLoadingTime(LoadSceneHandler handler)
    {
        if (handler.IsLoading)
        {
            if (Time.realtimeSinceStartup - handler.startLoadTime > 150)
            {
                Init.ReturnToStart();
            }
        }
    }

    public void UnloadAllWindow(LoadSceneHandler last, LoadSceneHandler cur)
    {
        Main.WindowService.ShowMaskBox((int)MaskType.WINDOW_ANIM, false);
        Main.WindowService.CloseAllWindow();

        if (cur.deepClean || (last != null && last.deepClean))
        {
            Main.WindowService.ReleaseAllWindow(ReleaseType.DEEPLY);
        }
        else
        {
            Main.WindowService.ReleaseAllWindow(ReleaseType.CHANGE_SCENE);
        }
    }
    
    public void OpenChangeScenePanel(LoadSceneHandler handler)
    {
        if (handler == null || handler.turnNode == null)
            return;

        WNode.TurnNode node = handler.turnNode;
        if (string.IsNullOrEmpty(node.nodeName))
            return;

        if (Main.WindowService.OpenSpecialWindow(node))
            return;

        if (node.nodeType == NodeType.ROOT)
        {
            Main.WindowService.OpenRoot(node.nodeName, node.nodeParam);
        }
        else
        {
            Main.WindowService.OpenWindow(node.nodeName, node.nodeParam);
        }

        handler.turnNode = null;
    }
}