namespace LccHotfix
{

    public class HandleSubobjKillCmdCfg : ICustomNodeCfg
    {
        public NodeParamAction Action;

        public HandleSubobjKillCmdCfg(NodeParamAction action)
        {
            Action = action;
        }
        public virtual System.Type NodeType() { return typeof(HandleSubobjKillCmd); }
    }
    
    //////////////////////////////////////////////////////////////////////////
    // 运行时节点 :
    //////////////////////////////////////////////////////////////////////////
    public class HandleSubobjKillCmd : CustomNode, IEntityCommandHandler
    {
        private HandleSubobjKillCmdCfg mCfg;
        public override void InitializeNode(ICustomNodeCfg cfg, in CustomNodeContext context)
        {
            base.InitializeNode(cfg, in context);
            mCfg = cfg as HandleSubobjKillCmdCfg;
        }

        public bool HandleEntityCommand(LogicEntity entity, EntityCommand cmd)
        {
            if (cmd.CmdType == EntityCmdType.Nt_Kill)
            {
                if (mCfg.Action != null)
                {
                    mCfg.Action(this);
                }
                return true;
            }
            return false;
        }
    }
    
}
