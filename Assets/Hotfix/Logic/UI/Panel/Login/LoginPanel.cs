using LccModel;
using UnityEngine.UI;

namespace LccHotfix
{
    [UIEventHandler(UIEventType.Login)]
    public class LoginUIEventHandler : UIEvent
    {
        public override void Publish()
        {
            PanelManager.Instance.ShowPanel(PanelType.Login);
        }
    }
    public class LoginModel : ViewModelBase
    {
        public bool isEnterMain;
    }
    public class LoginPanel : APanelView<LoginModel>
    {
        public Button testBtn;
        public override void InitView(LoginModel viewModel)
        {
            LogUtil.Debug("InitView第一个执行的函数");
            LogUtil.Debug("负责给viewModel的字段绑定");
            //参考
            //Binding<bool>(nameof(viewModel.isLoading), IsLoading);
            //public void IsLoading(bool oldValue, bool newValue)
            //{
            //}
        }
        public override void Binding(LoginModel oldValue, LoginModel newValue)
        {
            LogUtil.Debug("Binding第二个执行的函数");
            LogUtil.Debug("LoginModel第一次初始化时会触发，LoginModel绑定切换的时候会调用");
        }


        public override void OnInitData(Panel panel)
        {
            base.OnInitData(panel);
            LogUtil.Debug("OnInitData第三个执行的函数");

            panel.data.type = UIType.Normal;
            panel.data.showMode = UIShowMode.HideOther;
            panel.data.navigationMode = UINavigationMode.IgnoreNavigation;
        }

        public override void OnInitComponent(Panel panel)
        {
            LogUtil.Debug("OnInitComponent第四个执行的函数");

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
            ViewModel.isEnterMain = true;


        }
    }
}