using System;

namespace LccHotfix
{
    public static partial class NodeConfigTypeRegistry
    {
        static bool _DelegateBhvCfg = Register(typeof(DelegateBhvCfg), NodeCategory.Bhv);
    }

    public delegate void NodeParamAction(CustomNode node);

    public delegate void NodeParamTickAction(CustomNode node, float dt);

    public class DelegateBhvCfg : ICustomNodeCfg
    {
        public NodeParamTickAction NodeParamTickFunc { get; protected set; } //这个不宜配置到Seq中
        public NodeParamAction NodeParamFunc { get; protected set; }
        public Action SimpleFunc { get; protected set; }

        public System.Type NodeType()
        {
            return typeof(DelegateBhv);
        }

        public bool InitializeCall = false;
        public bool BeginCall = true;
        public bool DestroyCall = false;
        public bool UpdateCall = false;

        public DelegateBhvCfg(Action func)
        {
            SimpleFunc = func;
        }

        public DelegateBhvCfg(NodeParamAction func)
        {
            NodeParamFunc = func;
        }

        public DelegateBhvCfg(NodeParamTickAction func)
        {
            NodeParamTickFunc = func;
        }
    }

    /// <summary>
    /// 运行时：调用简单函数
    /// </summary>
    public class DelegateBhv : BehaviorNode<DelegateBhvCfg>
    {
        public override void InitializeNode(ICustomNodeCfg cfg, in CustomNodeContext context)
        {
            base.InitializeNode(cfg, context);
            if (mCfg != null && mCfg.InitializeCall)
            {
                CallAction();
            }
        }

        public override void Destroy()
        {
            if (mCfg != null && mCfg.DestroyCall)
            {
                CallAction();
            }

            base.Destroy();
        }

        protected override void OnBegin()
        {
            if (mCfg != null && mCfg.BeginCall)
            {
                CallAction();
            }
        }

        protected override float OnUpdate(float dt)
        {
            if (mCfg != null && mCfg.UpdateCall)
            {
                CallAction(dt);
            }

            return dt;
        }

        private void CallAction(float dt = 0f)
        {
            if (mCfg.NodeParamFunc != null)
            {
                mCfg.NodeParamFunc(this);
            }

            if (mCfg.SimpleFunc != null)
            {
                mCfg.SimpleFunc();
            }

            if (mCfg.NodeParamTickFunc != null)
            {
                mCfg.NodeParamTickFunc(this, dt);
            }
        }
    }
}