using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using Object = UnityEngine.Object;

namespace LccHotfix
{
    public abstract class UIPanel<T> : UIBinding<T>, IPanelHandler where T : ViewModelBase
    {
        public virtual void ShowTopPanel(TopType topType, string title)
        {
            ViewModel.topData.topType = topType;
            ViewModel.topData.title = title;
            //刷新Top
            PanelManager.Instance.ShowPanel(PanelType.Top, new ShowPanelData(false, false, ViewModel.topData, false, false, false));
        }

        public virtual void OnHidePanel()
        {
            PanelManager.Instance.HidePanel(ViewModel.selfPanel.Type);
        }

        public override void OnInitData(Panel panel)
        {
            ViewModel.selfPanel = panel;
            ViewModel.InitTopData();
        }
    }
}