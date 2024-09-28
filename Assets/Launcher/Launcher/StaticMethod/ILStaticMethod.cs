//using ILRuntime.CLR.Method;
//using ILRuntime.Runtime.Enviorment;

//namespace LccModel
//{
//    public class ILStaticMethod : AStaticMethod
//    {
//        private readonly AppDomain _appDomain;
//        private readonly IMethod _method;
//        private readonly object[] _param;

//        public ILStaticMethod(AppDomain appDomain, string typeName, string methodName, int paramsCount)
//        {
//            this._appDomain = appDomain;
//            _method = appDomain.GetType(typeName).GetMethod(methodName, paramsCount);
//            _param = new object[_method.ParameterCount];
//        }
//        public override void Run(params object[] param)
//        {
//            if (this._param.Length != param.Length)
//            {
//                LogUtil.Error("调用失败");
//                return;
//            }
//            for (int i = 0; i < param.Length; i++)
//            {
//                this._param[i] = param[i];
//            }
//            _appDomain.Invoke(_method, null, this._param);
//        }
//    }
//}