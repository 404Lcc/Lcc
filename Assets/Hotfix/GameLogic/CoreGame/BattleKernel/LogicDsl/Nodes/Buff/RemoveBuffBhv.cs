namespace LccHotfix
{
    /// <summary>
    /// 移除buff（驱散buff）
    /// </summary>
    public class RemoveBuffBhvCfg : ICustomNodeCfg
    {
        public int[] LogicID; // buff的逻辑id
        public int[] BuffTag; // buff的标签，mask格式

        public static RemoveBuffBhvCfg ByBuffTag(params int[] buffTag)
        {
            return new RemoveBuffBhvCfg() { BuffTag = buffTag };
        }

        public static RemoveBuffBhvCfg ByLogicID(params int[] logicID)
        {
            return new RemoveBuffBhvCfg() { LogicID = logicID };
        }

        public virtual System.Type NodeType() { return typeof(RemoveBuffBhv); }
    }

    //////////////////////////////////////////////////////////////////////////
    // 运行时节点 :
    //////////////////////////////////////////////////////////////////////////
    public class RemoveBuffBhv : BehaviorNode<RemoveBuffBhvCfg>
    {
        protected override void OnBegin()
        {
            base.OnBegin();
            var entity = this.GetOwnerEntity();
            if (!entity.hasComBuffCenter)
            {
                return;
            }
            if (_cfg.BuffTag != null && _cfg.BuffTag.Length > 0)
            {
                entity.comBuffCenter.RemoveBuffByBuffTag(_cfg.BuffTag);
            }
            if (_cfg.LogicID != null && _cfg.LogicID.Length > 0)
            {
                entity.comBuffCenter.RemoveBuffByID(_cfg.LogicID);
            }
        }
    }

}
