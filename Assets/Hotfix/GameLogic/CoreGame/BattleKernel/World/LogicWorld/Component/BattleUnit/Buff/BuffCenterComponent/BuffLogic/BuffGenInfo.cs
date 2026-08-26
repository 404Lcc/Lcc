using PBConfig;

namespace LccHotfix
{
    public delegate BuffGenInfo AdjustBuffGenInfo(BuffGenInfo genInfo);

    public class BuffGenInfo : CustomLogicGenInfo, IHasSumUnitSource, IHasSubobjectSource, IHasDamageType
    {
        public int BuffLevel;
        public int BuffMaxLevel;
        public float DurationAddSeconds; // 生成buff时固定增加的持续时间
        public float SourceDurationAddRate; // 生成buff时来自来源逻辑的持续时间加成比例
        public float DurationAddRate; // 额外的持续时间加成比例，用于buff免疫逻辑
        public LogicEntity Sourcer;
        public LogicEntity Owner;
        
        private UnitSource _sumUnitSource;
        public ref UnitSource SumUnitSource //attacker
        {
            get { return ref _sumUnitSource; }
        }

        private SubobjectSource? _subobjSource;
        public ref SubobjectSource? SubobjSource
        {
            get { return ref _subobjSource; }
        }

        private TElementType _damageType;
        public ref TElementType DamageType
        {
            get { return ref _damageType; }
        }
        
        
        // // 可空存储，未设置时 GetSubobjectSource 返回 false
        // public SubobjectSource? SubobjSource
        // {
        //     get { return _subobjSource; }
        //     set { _subobjSource = value; }
        // }
        
        public override void Destroy()
        {
            Clear();
            base.Destroy();
        }
        
        public void AdjustBuffGenInfo_FromUnitEntity(LogicEntity fromEntity)
        {
            PreEnv.WriteVar<long>(CvKey.CV_OwnerFighterEntityID, fromEntity.ID);

            var playerInfo = fromEntity.OwnerWorld?.GetCreationInfo<BattleKernelCreationInfo>()?.UnitOwnerInfoProvider?.GetOwnerInfo(fromEntity);
            if (playerInfo != null)
            {
                PreEnv.WriteVar(CvKey.CV_OwnerPlayerInfo, playerInfo);
            }

            if (fromEntity.hasComBattleUnitTag)
            {
                var battleUnitTID = fromEntity.comBattleUnitTag.Tag.BattleUnitTid ?? 0;
                PreEnv.WriteVar(CvKey.CV_BattleUnitTid, battleUnitTID);
            }
        }

        public void Clear()
        {
            BuffLevel = -1;
            BuffMaxLevel = -1;
            DurationAddSeconds = 0;
            SourceDurationAddRate = 0;
            DurationAddRate = 0;
            Sourcer = null;
            Owner = null;
        }

    }
}
