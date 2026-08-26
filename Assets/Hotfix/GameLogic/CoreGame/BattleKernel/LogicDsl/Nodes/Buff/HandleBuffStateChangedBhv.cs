namespace LccHotfix
{
    public class HandleBuffStateChangedBhvCfg : ICustomNodeCfg
    {
        public NodeParamAction Action;

        public HandleBuffStateChangedBhvCfg(NodeParamAction action)
        {
            Action = action;
        }

        public virtual System.Type NodeType() { return typeof(HandleBuffStateChangedBhv); }
    }

    //////////////////////////////////////////////////////////////////////////
    // 运行时节点 :
    //////////////////////////////////////////////////////////////////////////
    public class HandleBuffStateChangedBhv : BehaviorNode<HandleBuffStateChangedBhvCfg>, IBuffStateChanged
    {
        public override void InitializeNode(ICustomNodeCfg cfg, in CustomNodeContext context)
        {
            base.InitializeNode(cfg, in context);
        }

        protected override void OnBegin()
        {
            base.OnBegin();
        }

        public override void Destroy()
        {
            base.Destroy();
        }

        public void OnBuffStateChanged(LogicEntity e, BuffLogic buff)
        {
            if (_cfg.Action != null)
            {
                _cfg.Action(this);
            }
        }
    }

}
