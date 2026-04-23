using System;
using System.Reflection;

namespace LccHotfix
{
    public class SimpleFunctionCndCfg : ConditionBaseCfg
    {
        public string ClassName { get; protected set; }
        public string FuncName { get; protected set; }
        public object[] Parameters { get; protected set; }
        public bool FirstParamIsCurNode { get; set; } = false;

        public override System.Type NodeType()
        {
            return typeof(SimpleFunctionCnd);
        }

        public SimpleFunctionCndCfg()
        {
        }

        public SimpleFunctionCndCfg(string className, string funcName, bool hasNodeParam, params object[] parameters)
        {
            ClassName = className;
            FuncName = funcName;
            Parameters = parameters;
            FirstParamIsCurNode = hasNodeParam;
        }
    }

    //////////////////////////////////////////////////////////////////////////
    // 静态方法 bool返回值 条件
    // 主要用来减少创建简单的特化业务条件节点类
    //////////////////////////////////////////////////////////////////////////
    public class SimpleFunctionCnd : ConditionNodeBase
    {
        private MethodInfo _cachedMethod;
        private SimpleFunctionCndCfg _cfg;
        private object[] _params;

        public override void InitializeNode(ICustomNodeCfg cfg, in CustomNodeContext context)
        {
            base.InitializeNode(cfg, context);
            _cfg = cfg as SimpleFunctionCndCfg;
            if (_cachedMethod == null)
            {
                Type targetType = Type.GetType(_cfg.ClassName);
                _cachedMethod = targetType.GetMethod(_cfg.FuncName);
            }

            int cfgParamLen = _cfg.Parameters?.Length ?? 0;
            int paramCnt = cfgParamLen;
            if (_cfg.FirstParamIsCurNode)
            {
                paramCnt++;
            }

            _params = new object[paramCnt];
            int idx = 0;
            if (_cfg.FirstParamIsCurNode)
            {
                _params[idx] = this;
                ++idx;
            }

            for (int i = 0; i < cfgParamLen; i++)
            {
                _params[idx] = _cfg.Parameters[i];
                ++idx;
            }
        }

        public override void Destroy()
        {
            _cfg = null;
            _cachedMethod = null;
            _params = null;
            base.Destroy();
        }


        protected override bool Inner_ConditionCheck()
        {
            if (_cachedMethod != null && _cfg != null)
            {
                var result = _cachedMethod.Invoke(null, _params);
                bool boolResult = Convert.ToBoolean(result);
                return boolResult;
            }

            return false;
        }
    }
}