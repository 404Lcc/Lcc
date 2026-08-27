namespace LccHotfix
{
    public class ImmuneBhvCfg : ICustomNodeCfg
    {
        public enum ImmuneTypeEnum
        {
            Bool = 1, // 布尔类型免疫效果
            Permyriad = 2, // 万分比类型免疫效果
            BuffId = 3, // BuffId免疫效果
            BuffTag = 4, // Buff标签免疫效果
        }

        public int Flag;

        public ImmuneTypeEnum ImmuneType;
        public int ImmuneId;
        public int ImmuneValue;

        public ImmuneBhvCfg(int flag)
        {
            Flag = flag;
        }

        public ImmuneBhvCfg WithBool(int id)
        {
            ImmuneType = ImmuneTypeEnum.Bool;
            ImmuneId = id;
            return this;
        }

        public ImmuneBhvCfg WithPermyriad(int id, int value)
        {
            ImmuneType = ImmuneTypeEnum.Permyriad;
            ImmuneId = id;
            ImmuneValue = value;
            return this;
        }

        public ImmuneBhvCfg WithBuffId(int buffId, int value)
        {
            ImmuneType = ImmuneTypeEnum.BuffId;
            ImmuneId = buffId;
            ImmuneValue = value;
            return this;
        }

        public ImmuneBhvCfg WithBuffTag(int buffTag, int value)
        {
            ImmuneType = ImmuneTypeEnum.BuffTag;
            ImmuneId = buffTag;
            ImmuneValue = value;
            return this;
        }

        public virtual System.Type NodeType() { return typeof(ImmuneBhv); }
    }

    //////////////////////////////////////////////////////////////////////////
    // 运行时节点 :
    //////////////////////////////////////////////////////////////////////////
    public class ImmuneBhv : BehaviorNode<ImmuneBhvCfg>
    {
        public override void InitializeNode(ICustomNodeCfg cfg, in CustomNodeContext context)
        {
            base.InitializeNode(cfg, in context);
        }

        protected override void OnBegin()
        {
            base.OnBegin();
            var entity = this.GetOwnerEntity();
            if (entity is null)
            {
                return;
            }
            if (!entity.hasComImmune)
            {
                entity.AddComImmune();
            }
            switch (_cfg.ImmuneType)
            {
                case ImmuneBhvCfg.ImmuneTypeEnum.Bool:
                    entity.comImmune.AddBoolImmune(_cfg.ImmuneId, true, _cfg.Flag);
                    break;
                case ImmuneBhvCfg.ImmuneTypeEnum.Permyriad:
                    entity.comImmune.AddPermyriadImmune(_cfg.ImmuneId, _cfg.ImmuneValue, _cfg.Flag);
                    break;
                case ImmuneBhvCfg.ImmuneTypeEnum.BuffId:
                    entity.comImmune.AddBuffImmune(_cfg.ImmuneId, _cfg.ImmuneValue, _cfg.Flag);
                    break;
                case ImmuneBhvCfg.ImmuneTypeEnum.BuffTag:
                    entity.comImmune.AddBuffTagImmune(_cfg.ImmuneId, _cfg.ImmuneValue, _cfg.Flag);
                    break;
            }
        }

        public override void Destroy()
        {
            var entity = this.GetOwnerEntity();
            if (entity is null || !entity.hasComImmune)
            {
                base.Destroy();
                return;
            }
            switch (_cfg.ImmuneType)
            {
                case ImmuneBhvCfg.ImmuneTypeEnum.Bool:
                    entity.comImmune.RemoveBoolImmune(_cfg.ImmuneId, _cfg.Flag);
                    break;
                case ImmuneBhvCfg.ImmuneTypeEnum.Permyriad:
                    entity.comImmune.RemovePermyriadImmune(_cfg.ImmuneId, _cfg.Flag);
                    break;
                case ImmuneBhvCfg.ImmuneTypeEnum.BuffId:
                    entity.comImmune.RemoveBuffImmune(_cfg.ImmuneId, _cfg.Flag);
                    break;
                case ImmuneBhvCfg.ImmuneTypeEnum.BuffTag:
                    entity.comImmune.RemoveBuffTagImmune(_cfg.ImmuneId, _cfg.Flag);
                    break;
            }
            base.Destroy();
        }
    }

}
