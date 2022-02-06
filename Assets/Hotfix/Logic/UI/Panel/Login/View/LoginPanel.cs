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
        public override void Awake()
        {
            LogUtil.Log("Awake第三个执行的函数");
        }
        public override void Start()
        {
            LogUtil.Log("调用base自动赋值字段");
            base.Start();
            LogUtil.Log("Start第四个执行的函数");
        }
        public override void InitData(object[] datas)
        {
            base.InitData(datas);
            LogUtil.Log("InitData第五个执行的函数");
        }
        public override void FixedUpdate()
        {
            LogUtil.Log("FixedUpdate");
        }
        public override void Update()
        {
            LogUtil.Log("Update");
        }
        public override void LateUpdate()
        {
            LogUtil.Log("LateUpdate");
        }
    }
}