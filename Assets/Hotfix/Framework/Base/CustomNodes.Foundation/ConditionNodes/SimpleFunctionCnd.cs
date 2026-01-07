using System;
using System.Reflection;
using System.Xml;

namespace LccHotfix
{
    public static partial class NodeConfigTypeRegistry
    {
        static bool _SimpleFunctionCndCfg = Register(typeof(SimpleFunctionCndCfg), NodeCategory.Cnd);
    }

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

        public override bool ParseFromXml(XmlNode xmlNode)
        {
            var str = XmlHelper.GetAttribute(xmlNode, "ClassName");
            CLHelper.Assert(!string.IsNullOrEmpty(str));
            ClassName = str;
            var funcStr = XmlHelper.GetAttribute(xmlNode, "FuncName");
            CLHelper.Assert(!string.IsNullOrEmpty(funcStr));
            FuncName = funcStr;

            return base.ParseFromXml(xmlNode);
        }
    }

    //////////////////////////////////////////////////////////////////////////
    // 静态方法 bool返回值 条件
    // 主要用来减少创建简单的特化业务条件节点类
    //////////////////////////////////////////////////////////////////////////
    public class SimpleFunctionCnd : ConditionNodeBase
    {
        private MethodInfo mCachedMethod;
        private SimpleFunctionCndCfg mCfg;
        public object[] mParams;

        public override void InitializeNode(ICustomNodeCfg cfg, in CustomNodeContext context)
        {
            base.InitializeNode(cfg, context);
            mCfg = cfg as SimpleFunctionCndCfg;
            if (mCachedMethod == null)
            {
                Type targetType = Type.GetType(mCfg.ClassName);
                mCachedMethod = targetType.GetMethod(mCfg.FuncName);
            }

            int cfgParamLen = mCfg.Parameters?.Length ?? 0;
            int paramCnt = cfgParamLen;
            if (mCfg.FirstParamIsCurNode)
            {
                paramCnt++;
            }

            mParams = new object[paramCnt];
            int idx = 0;
            if (mCfg.FirstParamIsCurNode)
            {
                mParams[idx] = this;
                ++idx;
            }

            for (int i = 0; i < cfgParamLen; i++)
            {
                mParams[idx] = mCfg.Parameters[i];
                ++idx;
            }
        }

        public override void Destroy()
        {
            mCfg = null;
            mCachedMethod = null;
            mParams = null;
            base.Destroy();
        }


        protected override bool Inner_ConditionCheck()
        {
            if (mCachedMethod != null && mCfg != null)
            {
                var result = mCachedMethod.Invoke(null, mParams);
                bool boolResult = Convert.ToBoolean(result);
                return boolResult;
            }

            return false;
        }

        public static bool TestInvokeStatic1(int a)
        {
            LogWrapper.LogInfo($"TestInvokeStatic1 a={a}");
            return true;
        }
    }
}