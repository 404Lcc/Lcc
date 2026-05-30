using System;
using System.Collections.Generic;
using System.Xml;

namespace LccHotfix
{
    


    public class HandleBuffEntityCmdBhvCfg : ICustomNodeCfg
    {
        public System.Type NodeType() { return typeof(HandleBuffEntityCmdBhv); }

        public int EntityCmdType;
        public NodeParamEntityCmdAction Action;
        public HandleBuffEntityCmdBhvCfg(int entityCmdType, NodeParamEntityCmdAction action)
        {
            EntityCmdType = entityCmdType;
            Action = action;
        }
    }

    //////////////////////////////////////////////////////////////////////////
    // 运行时节点 :
    //////////////////////////////////////////////////////////////////////////
    public class HandleBuffEntityCmdBhv : BehaviorNode<HandleBuffEntityCmdBhvCfg>, IEntityCommandHandler
    {
        public bool HandleEntityCommand(LogicEntity entity, EntityCommand cmd)
        {
            if (cmd.CmdType == _cfg.EntityCmdType)
            {
                if (_cfg.Action != null)
                {
                    return _cfg.Action(this, cmd);
                }
            }
            return false;
        }
    }
}
