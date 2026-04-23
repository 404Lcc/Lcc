using System;

namespace LccHotfix
{
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
            if (_cfg != null && _cfg.InitializeCall)
            {
                CallAction();
            }
        }

        public override void Destroy()
        {
            if (_cfg != null && _cfg.DestroyCall)
            {
                CallAction();
            }

            base.Destroy();
        }

        protected override void OnBegin()
        {
            if (_cfg != null && _cfg.BeginCall)
            {
                CallAction();
            }
        }

        protected override float OnUpdate(float dt)
        {
            if (_cfg != null && _cfg.UpdateCall)
            {
                CallAction(dt);
            }

            return dt;
        }

        private void CallAction(float dt = 0f)
        {
            if (_cfg.NodeParamFunc != null)
            {
                _cfg.NodeParamFunc(this);
            }

            if (_cfg.SimpleFunc != null)
            {
                _cfg.SimpleFunc();
            }

            if (_cfg.NodeParamTickFunc != null)
            {
                _cfg.NodeParamTickFunc(this, dt);
            }
        }
    }
}