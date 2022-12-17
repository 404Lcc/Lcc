using ILRuntime.CLR.Method;
using ILRuntime.Runtime.Enviorment;

namespace LccModel
{
    public class ILStaticMethod : AStaticMethod
    {
        private readonly AppDomain appDomain;
        private readonly IMethod method;
        private readonly object[] param;

        public ILStaticMethod(AppDomain appDomain, string typeName, string methodName, int paramsCount)
        {
            this.appDomain = appDomain;
            method = appDomain.GetType(typeName).GetMethod(methodName, paramsCount);
            param = new object[method.ParameterCount];
        }
        public override void Run(params object[] param)
        {
            if (this.param.Length != param.Length)
            {
                LogUtil.Error("µ÷ÓÃÊ§°Ü");
                return;
            }
            for (int i = 0; i < param.Length; i++)
            {
                this.param[i] = param[i];
            }
            appDomain.Invoke(method, null, this.param);
        }
    }
}