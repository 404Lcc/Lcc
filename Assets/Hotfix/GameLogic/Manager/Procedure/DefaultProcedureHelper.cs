using System.Collections;
using LccModel;
using UnityEngine;

namespace LccHotfix
{
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
            Launcher.Instance.GameControl.SetGameSpeed(1);
            Launcher.Instance.GameControl.ChangeFPS();
        }

        public void UnloadAllPanel(LoadProcedureHandler last, LoadProcedureHandler cur)
        {
            Main.WindowService.HideAllWindow();

            if (cur.deepClean || (last != null && last.deepClean))
            {
                Main.WindowService.ReleaseAllWindow(ReleaseType.DEEPLY);
            }
            else
            {
                Main.WindowService.ReleaseAllWindow(ReleaseType.CHANGE_PROCEDURE);
            }
        }

        public void OpenChangeProcedurePanel(LoadProcedureHandler handler)
        {
            if (handler == null || handler.turnNode == null)
                return;

            TurnNode node = handler.turnNode;
            if (string.IsNullOrEmpty(node.nodeName))
                return;

            if (Main.UIService.OpenSpecialPanel(node))
                return;

            if (node.nodeType == NodeType.Domain)
            {
                Main.WindowService.OpenRoot(node.nodeName, node.nodeParam);
            }
            else
            {
                Main.WindowService.OpenWindow(node.nodeName, node.nodeParam);
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
}