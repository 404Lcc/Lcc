namespace LccHotfix
{
    public class ModifyAttributeBhvCfg<TValue> : ICustomNodeCfg
    {
        public int AttributeName;
        public TValue Value;
        public int Flag;
        public string ValueKey;
        
        public ModifyAttributeBhvCfg(int attrName, TValue value, int flag)
        {
            AttributeName = attrName;
            Value = value;
            Flag = flag;
        }

        public ModifyAttributeBhvCfg(int attrName, string value, int flag)
        {
            AttributeName = attrName;
            ValueKey = value;
            Flag = flag;
        }
        
        public virtual System.Type NodeType() { return typeof(ModifyAttributeBhv<TValue>); }
    }
    
    //////////////////////////////////////////////////////////////////////////
    // 运行时节点 :
    //////////////////////////////////////////////////////////////////////////
    public class ModifyAttributeBhv<TValue> : BehaviorNodeBase
    {
        private ModifyAttributeBhvCfg<TValue> mCfg;
        
        public override void InitializeNode(ICustomNodeCfg cfg, in CustomNodeContext context)
        {
            base.InitializeNode(cfg, in context);
            mCfg = cfg as ModifyAttributeBhvCfg<TValue>;
        }

        protected override void OnBegin()
        {
            base.OnBegin();
            TValue value = mCfg.Value;
            if (!string.IsNullOrEmpty(mCfg.ValueKey))
            {
                value = GetVar<TValue>(mCfg.ValueKey);
            }
            var logicEntity = GetVar<LogicEntity>(CvKey.CV_OwnerEntity);
            if (logicEntity.IsDead())
                return;
            if (logicEntity.hasComAttributes)
            {
                logicEntity.comAttributes.Modify<TValue>(mCfg.AttributeName, value, mCfg.Flag);
            }
        }

        public override void Destroy()
        {
            var logicEntity = GetVar<LogicEntity>(CvKey.CV_OwnerEntity);
            if (logicEntity.IsDead())
            {
                return;
            }
            if (logicEntity.hasComAttributes)
            {
                logicEntity.comAttributes.RemoveModify<TValue>(mCfg.AttributeName, mCfg.Flag);
            }
            base.Destroy();
        }
    }
    
}
