using LccModel;

namespace LccHotfix
{
    public class LoginPanel : APanelView<LoginModel>
    {
        public override void InitView(LoginModel viewModel)
        {
            LogUtil.Log("InitView第一个执行的函数");
            LogUtil.Log("负责给viewModel的字段绑定");
            //参考
            //Binding<bool>(nameof(viewModel.isLoading), IsLoading);
            //public void IsLoading(bool oldValue, bool newValue)
            //{
            //}
        }
        public override void Binding(LoginModel oldValue, LoginModel newValue)
        {
            LogUtil.Log("Binding第二个执行的函数");
            LogUtil.Log("LoginModel第一次初始化时会触发，LoginModel绑定切换的时候会调用");
        }



    }
}