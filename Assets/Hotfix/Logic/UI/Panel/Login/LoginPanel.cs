using LccModel;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace LccHotfix
{
    //public class LoginItemData
    //{
    //}
    //public class LoginItem : LoopScrollItem
    //{
    //}

    [UIEventHandler(UIEventType.Login)]
    public class LoginUIEventHandler : UIEvent
    {
        public override void Publish()
        {
            PanelManager.Instance.ShowPanel(PanelType.UILogin);
        }
    }
    public class UILoginModel : ViewModelBase
    {
    }
    public class UILoginPanel : UIPanel<UILoginModel>
    {
        public Button testBtn;
        public LoopScrollRect loop;
        public GameObject item;
        //public LoopScroll<LoginItemData, LoginItem> loopScroll;
        //public override void InitView(LoginModel viewModel)
        //{
        //    LogUtil.Debug("InitView第一个执行的函数");
        //    LogUtil.Debug("负责给viewModel的字段绑定");
        //    //参考
        //    //Binding<bool>(nameof(viewModel.isLoading), IsLoading);
        //    //public void IsLoading(bool oldValue, bool newValue)
        //    //{
        //    //}
        //}
        //public override void Binding(LoginModel oldValue, LoginModel newValue)
        //{
        //    LogUtil.Debug("Binding第二个执行的函数");
        //    LogUtil.Debug("LoginModel第一次初始化时会触发，LoginModel绑定切换的时候会调用");
        //}

        public override void OnInitComponent(Panel panel)
        {
            base.OnInitComponent(panel);
            LogUtil.Debug("OnInitComponent第三个执行的函数");

            //loopScroll = panel.AddChildren<LoopScroll<LoginItemData, LoginItem>>(loop, item);
        }
        public override void OnInitData(Panel panel)
        {
            base.OnInitData(panel);
            LogUtil.Debug("OnInitData第四个执行的函数");

            panel.data.type = UIType.Normal;
            panel.data.showMode = UIShowMode.HideOther;
            panel.data.navigationMode = UINavigationMode.IgnoreNavigation;
        }

        public override void OnRegisterUIEvent(Panel panel)
        {
            LogUtil.Debug("OnRegisterUIEvent第五个执行的函数");

            testBtn.onClick.AddListener(OnEnterMain);
        }


        public override void OnShow(Panel panel, AObjectBase contextData = null)
        {
            base.OnShow(panel, contextData);
            LogUtil.Debug("OnShow第六个执行的函数");

            UpdatePanel.Instance.Hide();

            //List<LoginItemData> list = new List<LoginItemData>();
            //for (int i = 0; i < 10; i++)
            //{
            //    list.Add(new LoginItemData());
            //}
            //loopScroll.Refill(list, 1);

            //for (int i = 0; i < 15; i++)
            //{
            //    loopScroll.AddData(new LoginItemData(), true);
            //}
        }

        public override void OnHide(Panel panel)
        {
            LogUtil.Debug("OnHide");
        }

        public override void OnBeforeUnload(Panel panel)
        {
            LogUtil.Debug("BeforeUnload");
        }
        public void OnEnterMain()
        {
            //List<LoginItemData> list = new List<LoginItemData>();
            //for (int i = 0; i < 10; i++)
            //{
            //    list.Add(new LoginItemData());
            //}
            //loopScroll.Refill(list, list.Count);
            ModelManager.Instance.GetModel<LoginModel>().OnEnterMain();
        }
    }
}