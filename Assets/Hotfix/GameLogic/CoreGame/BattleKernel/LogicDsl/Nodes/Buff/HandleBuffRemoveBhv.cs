namespace LccHotfix
{
    public class HandleBuffRemoveBhvCfg : ICustomNodeCfg
    {
        public NodeParamAction Action;
        
        public HandleBuffRemoveBhvCfg(NodeParamAction action)
        {
            Action = action;
        }
        
        public virtual System.Type NodeType() { return typeof(HandleBuffRemoveBhv); }
    }
    
    //////////////////////////////////////////////////////////////////////////
    // 运行时节点 :
    //////////////////////////////////////////////////////////////////////////
    public class HandleBuffRemoveBhv : BehaviorNode<HandleBuffRemoveBhvCfg>, IBuffPreviousRemove
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

        public void OnBuffPreviousRemove()
        {
            if (_cfg.Action != null)
            {
                _cfg.Action(this);
            }
        }
    }
    
}
