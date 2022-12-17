using System.Reflection;

namespace LccModel
{
    public class MonoStaticMethod : AStaticMethod
    {
        private readonly MethodInfo methodInfo;

        private readonly object[] param;

        public MonoStaticMethod(Assembly assembly, string typeName, string methodName)
        {
            methodInfo = assembly.GetType(typeName).GetMethod(methodName);
            param = new object[methodInfo.GetParameters().Length];
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
            methodInfo.Invoke(null, this.param);
        }
    }
}