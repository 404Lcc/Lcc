namespace LccHotfix
{
    public class HandleBuffAddBhvCfg : ICustomNodeCfg
    {
        public NodeParamAction Action;
        
        public HandleBuffAddBhvCfg(NodeParamAction action)
        {
            Action = action;
        }
        
        public virtual System.Type NodeType() { return typeof(HandleBuffAddBhv); }
    }
    
    //////////////////////////////////////////////////////////////////////////
    // 运行时节点 :
    //////////////////////////////////////////////////////////////////////////
    public class HandleBuffAddBhv : BehaviorNode<HandleBuffAddBhvCfg>, IEntityAddBuffNotify
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

        public void OnEntityAddBuff(LogicEntity e)
        {
            if (_cfg.Action != null)
            {
                _cfg.Action(this);
            }
        }
    }
    
}
