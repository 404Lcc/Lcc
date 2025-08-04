using System.Collections;
using LccHotfix;
using LccModel;
using UnityEngine;
using Init = LccHotfix.Init;

public class DefaultProcedureHelper : IProcedureHelper
{
    public void UpdateLoadingTime(LoadProcedureHandler handler)
    {
        if (handler.IsLoading)
        {
            if (Time.realtimeSinceStartup - handler.startLoadTime > 150)
            {
                Init.ReturnToStart();
            }
        }
    }
    public void ResetSpeed()
    {
        Launcher.Instance.SetGameSpeed(1);
        Launcher.Instance.ChangeFPS();
    }
    public void UnloadAllPanel(LoadProcedureHandler last, LoadProcedureHandler cur)
    {
        UI.ShowMaskBox((int)MaskType.WINDOW_ANIM, false);
        UI.CloseAll();

        if (cur.deepClean || (last != null && last.deepClean))
        {
            UI.Release(ReleaseType.DEEPLY);
        }
        else
        {
            UI.Release(ReleaseType.CHANGE_PROCEDURE);
        }
    }
    
    public void OpenChangeProcedurePanel(LoadProcedureHandler handler)
    {
        if (handler == null || handler.turnNode == null)
            return;

        WNode.TurnNode node = handler.turnNode;
        if (string.IsNullOrEmpty(node.nodeName))
            return;

        if (Main.UIService.OpenSpecialPanel(node))
            return;

        if (node.nodeType == NodeType.ROOT)
        {
            UI.OpenRoot(node.nodeName, node.nodeParam);
        }
        else
        {
            UI.OpenWindow(node.nodeName, node.nodeParam);
        }

        handler.turnNode = null;
    }
    
    public IEnumerator ShowProcedureLoading(LoadingType loadType)
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
}