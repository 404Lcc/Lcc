namespace LccHotfix
{
    public delegate void BuffHandleBeforeDmgAction(CustomNode node, DamageContext context, ref DamageResult result);
    public class HandleBuffBeforeDmgBhvCfg : ICustomNodeCfg
    {
        public BuffHandleBeforeDmgAction Action;
        
        public HandleBuffBeforeDmgBhvCfg(BuffHandleBeforeDmgAction action)
        {
            Action = action;
        }
        
        public virtual System.Type NodeType() { return typeof(HandleBuffBeforeDmgBhv); }
    }
    
    //////////////////////////////////////////////////////////////////////////
    // 运行时节点 :
    //////////////////////////////////////////////////////////////////////////
    public class HandleBuffBeforeDmgBhv : BehaviorNode<HandleBuffBeforeDmgBhvCfg>, IBuffHandleBeforeDmg
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
        
        public void HandleBeforeDmg(DamageContext context, ref DamageResult result)
        {
            if (_cfg.Action != null)
            {
                _cfg.Action(this, context, ref result);
            }
        }
    }
    
}
