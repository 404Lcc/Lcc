namespace LccHotfix
{
    /// <summary>
    /// 修改速度（按比例修改，加法叠加）
    /// </summary>
    public class ModifySpeedBhvCfg : ICustomNodeCfg
    {
        public float Value;
        public int Flag;
        public string ValueKey;
        public ModifySpeedBhvCfg(float value, int flag)
        {
            Value = value;
            Flag = flag;
        }

        public ModifySpeedBhvCfg(string value, int flag) 
        {
            ValueKey = value;
            Flag = flag;
        }
        
        public System.Type NodeType() { return typeof(ModifySpeedBhv); }
    }
    
    //////////////////////////////////////////////////////////////////////////
    // 运行时节点 :
    //////////////////////////////////////////////////////////////////////////
    public class ModifySpeedBhv : BehaviorNodeBase
    {
        private ModifySpeedBhvCfg mCfg;
        
        public override void InitializeNode(ICustomNodeCfg cfg, in CustomNodeContext context)
        {
            base.InitializeNode(cfg, in context);
            mCfg = cfg as ModifySpeedBhvCfg;
        }

        protected override void OnBegin()
        {
            base.OnBegin();
            var value = mCfg.Value;
            if (!string.IsNullOrEmpty(mCfg.ValueKey))
            {
                value = GetVar<float>(mCfg.ValueKey);
            }
            var logicEntity = GetVar<LogicEntity>(CvKey.CV_OwnerEntity);
            if (value < 0 && logicEntity.IsImmuneToSlowDown)
            {
                return;
            }
            if (logicEntity.hasComAttributes)
            {
                logicEntity.comAttributes.Modify<float>(PropertyFloat.MoveSpeed,
                    value * logicEntity.comAttributes.GetDefaultValue<float>(PropertyFloat.MoveSpeed), mCfg.Flag);
            }
        }

        public override void Destroy()
        {
            var logicEntity = GetVar<LogicEntity>(CvKey.CV_OwnerEntity);
            if (logicEntity.hasComAttributes)
            {
                logicEntity.comAttributes.RemoveModify<float>(PropertyFloat.MoveSpeed, mCfg.Flag);
            }
            base.Destroy();
        }
    }
    
}
