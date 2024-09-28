using System.Reflection;
using UnityEngine;

namespace LccModel
{
    public class MonoStaticMethod : AStaticMethod
    {
        private readonly MethodInfo _methodInfo;

        private readonly object[] _param;

        public MonoStaticMethod(Assembly assembly, string typeName, string methodName)
        {
            _methodInfo = assembly.GetType(typeName).GetMethod(methodName);
            _param = new object[_methodInfo.GetParameters().Length];
        }
        public override void Run(params object[] param)
        {
            if (this._param.Length != param.Length)
            {
                Debug.LogError("调用失败");
                return;
            }
            for (int i = 0; i < param.Length; i++)
            {
                this._param[i] = param[i];
            }
            _methodInfo.Invoke(null, this._param);
        }
    }
}