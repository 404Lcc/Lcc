namespace LccHotfix
{
    public class HandleBuffUpdateBhvCfg : ICustomNodeCfg
    {
        public NodeParamAction Action;
        
        public HandleBuffUpdateBhvCfg(NodeParamAction action)
        {
            Action = action;
        }
        
        public virtual System.Type NodeType() { return typeof(HandleBuffUpdateBhv); }
    }
    
    //////////////////////////////////////////////////////////////////////////
    // 运行时节点 :
    //////////////////////////////////////////////////////////////////////////
    public class HandleBuffUpdateBhv : BehaviorNode<HandleBuffUpdateBhvCfg>, IBuffUpgrade
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

        public void OnBuffUpgrade(LogicEntity e)
        {
            if (_cfg.Action != null)
            {
                _cfg.Action(this);
            }
        }

        public void OnBuffUpgrade(LogicEntity e, BuffLogic buff)
        {
            SetVar<BuffLogic>(CvKey.CV_CurBuff, buff);
            if (_cfg.Action != null)
            {
                _cfg.Action(this);
            }
        }
    }
    
}
