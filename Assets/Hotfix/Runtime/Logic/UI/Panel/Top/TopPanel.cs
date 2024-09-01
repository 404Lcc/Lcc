using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace LccHotfix
{
    public enum TopType
    {
        None = 0,
        CloseBtn = 1 << 1,
        HelpBtn = 1 << 2,
        Gold = 1 << 3,
    }
    public class TopData : AObjectBase
    {
        public int curPanel;
        public TopType topType;
        public string title;

        public override void InitData(object[] datas)
        {
            base.InitData(datas);

            curPanel = (int)datas[0];
        }
    }
    public class UITopModel : ViewModelBase
    {
        public int CurPanel => topData.curPanel;
        public TopType TopType => topData.topType;
        public string Title => topData.title;
    }
    public class UITopPanel : UIPanel<UITopModel>
    {
        public Button closeBtn;
        public TMP_Text titleText;
        public GameObject gold;
        public override void OnInitComponent(Panel panel)
        {
            base.OnInitComponent(panel);

        }
        public override void OnInitData(Panel panel)
        {
            base.OnInitData(panel);

            panel.data.type = UIType.Fixed;
            panel.data.showMode = UIShowMode.Normal;
            panel.data.navigationMode = UINavigationMode.IgnoreNavigation;
        }

        public override void OnShow(Panel panel, object contextData = null)
        {
            base.OnShow(panel, contextData);
            if (contextData is TopData top)
            {
                ViewModel.topData.curPanel = top.curPanel;
                ViewModel.topData.topType = top.topType;
                ViewModel.topData.title = top.title;
                UpdateEnable();
            }


        }
        public override void OnRegisterUIEvent(Panel panel)
        {
            base.OnRegisterUIEvent(panel);

            closeBtn.onClick.AddListener(OnClose);
        }
        public void OnClose()
        {
            PanelManager.Instance.PopupNavigationPanel();
        }



        private void UpdateEnable()
        {
            closeBtn.gameObject.SetActive(ViewModel.TopType.HasFlag(TopType.CloseBtn) && (ViewModel.CurPanel != (int)PanelType.None));

            gold.SetActive(ViewModel.TopType.HasFlag(TopType.Gold));

            titleText.text = ViewModel.Title;
        }
    }
}