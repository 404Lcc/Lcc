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

        public virtual void OnInitData(Panel panel)
        {
            ViewModel.selfPanel = panel;
            ViewModel.InitTopData();
        }
        public virtual void OnInitComponent(Panel panel)
        {
            InitComponent(panel.GameObject);
        }
        public virtual void OnRegisterUIEvent(Panel panel)
        {
        }

        public virtual void OnShow(Panel panel, AObjectBase contextData = null)
        {
            UpdateDepth();
        }
        public virtual void OnHide(Panel panel)
        {
        }
        public virtual void OnBeforeUnload(Panel panel)
        {
        }

        public virtual void OnReset(Panel panel)
        {
        }

        public virtual bool IsReturn(Panel panel)
        {
            return false;
        }
    }
}