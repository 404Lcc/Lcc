using PBConfig;

namespace LccHotfix
{


    ////////////////////////////////////////////////////////////////////////
    /// 子物体逻辑：基础版本
    ////////////////////////////////////////////////////////////////////////
    public class SubobjectGenInfo : CustomLogicGenInfo,
        IHasLogicWorld,
        IHasMetaWorld,
        IHasOwnerEntity,
        IHasOwnerFighterEntityID,
        IHasOwnerPlayerInfo,
        IHasSumUnitSource,
        IHasSubobjectSource,
        IHasDamageType
    {
        private UnitSource _sumUnitSource;

        public LogicWorld LogicWorld { get; protected set; }
        public MetaWorld MetaWorld { get; protected set; }
        public LogicEntity OwnerEntity { get; protected set; }      //子物体自身
        public long OwnerFighterEntityID { get; protected set; }    //子物体归属战斗单位
        public IBattlePlayerInfo OwnerPlayerInfo { get; protected set; }
        public uint BattleSupplyId { get; set; }
        public ref UnitSource SumUnitSource => ref _sumUnitSource;
        
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

        public override void Destroy()
        {
            LogicWorld = null;
            MetaWorld = null;
            OwnerEntity = null;
            OwnerFighterEntityID = 0;
            OwnerPlayerInfo = null;
            BattleSupplyId = 0;
            SumUnitSource = default;
            SubobjSource = default;
            _damageType = default;
            base.Destroy();
        }

        internal void Init(
            MetaWorld metaWorld,
            LogicEntity ownerEntity,
            long ownerFighterEntityID,
            UnitSource sumUnitSource,
            SubobjectSource subobjSource,
            TElementType damageType)
        {
            if (metaWorld == null)
                BattleLogger.LogError("SubobjectGenInfo Init 异常, MetaWorld 为空");
            if (ownerEntity == null)
                BattleLogger.LogError("SubobjectGenInfo Init 异常, OwnerEntity 为空");

            var logicWorld = ownerEntity.OwnerWorld;
            if (logicWorld == null)
                BattleLogger.LogError("SubobjectGenInfo Init 异常, LogicWorld 为空");
            if (ownerFighterEntityID == 0)
                BattleLogger.LogError("SubobjectGenInfo Init 异常, OwnerFighterEntityID 为 0");

            LogicWorld = logicWorld;
            MetaWorld = metaWorld;
            OwnerEntity = ownerEntity;
            OwnerFighterEntityID = ownerFighterEntityID;
            OwnerPlayerInfo = ownerEntity.hasComOwnerPlayer
                ? ownerEntity.comOwnerPlayer.PlayerInfoRef
                : ownerEntity.OwnerWorld?.GetCreationInfo<BattleKernelCreationInfo>()?.UnitOwnerInfoProvider?.GetOwnerInfo(ownerEntity);
            SumUnitSource = sumUnitSource;
            SubobjSource = subobjSource;
            DamageType = damageType;
            
            if (OwnerPlayerInfo == null)
                BattleLogger.LogError("SubobjectGenInfo Init 异常, OwnerPlayerInfo 为空");
        }

        // 黑板 key 已存在时跳过写入并打错误日志
        protected void WriteVarIfAbsent<T>(ref VarEnv varEnv, string key, T value)
        {
            if (!varEnv.HasVar<T>(key))
                varEnv.WriteVar<T>(key, value);
            else
                BattleLogger.LogError($"SubobjectGenInfo CopyToPreVarEnv 出现异常, 外部有冗余 Key={key}");
        }

        public override VarEnv CopyToPreVarEnv(ref VarEnv varEnv)
        {
            if (OwnerEntity == null)
            {
                BattleLogger.LogError("SubobjectGenInfo CopyToPreVarEnv 异常, 未调用 Init, OwnerEntity 为空");
                return base.CopyToPreVarEnv(ref varEnv);
            }
            WriteVarIfAbsent(ref varEnv, CvKey.CV_LogicWorld, LogicWorld);
            WriteVarIfAbsent(ref varEnv, CvKey.CV_MetaWorld, MetaWorld);
            WriteVarIfAbsent(ref varEnv, CvKey.CV_OwnerEntity, OwnerEntity);
            WriteVarIfAbsent(ref varEnv, CvKey.CV_OwnerFighterEntityID, OwnerFighterEntityID);
            WriteVarIfAbsent(ref varEnv, CvKey.CV_OwnerPlayerInfo, OwnerPlayerInfo);
            return base.CopyToPreVarEnv(ref varEnv);
        }

        /// <summary>
        /// 从对象池创建并初始化基础子物体逻辑所需的 GenInfo。
        /// </summary>
        internal static SubobjectGenInfo New(
            ICustomLogicService svc,
            MetaWorld metaWorld,
            LogicEntity ownerEntity,
            long ownerFighterEntityID,
            UnitSource sumUnitSource,
            SubobjectSource subobjSource,
            TElementType damageType)
        {
            var genInfo = svc.NewGenInfo<SubobjectGenInfo>();
            genInfo.Init(metaWorld, ownerEntity, ownerFighterEntityID, sumUnitSource, subobjSource, damageType);
            return genInfo;
        }
    }



    ////////////////////////////////////////////////////////////////////////
    /// 子物体逻辑：战斗单位版本
    ////////////////////////////////////////////////////////////////////////
    public class FighterSubobjectGenInfo : SubobjectGenInfo, IHasFighter
    {
        public int BattleUnitTid { get; protected set; }
        public TFighter FighterCfg { get; protected set; }


        public override void Destroy()
        {
            BattleUnitTid = 0;
            FighterCfg = null;
            base.Destroy();
        }

        internal void Init(
            MetaWorld metaWorld,
            LogicEntity ownerEntity,
            long ownerFighterEntityID,
            UnitSource sumUnitSource,
            SubobjectSource subobjSource,
            int battleUnitTid,
            TFighter fighterCfg,
            TElementType damageType)
        {
            base.Init(metaWorld, ownerEntity, ownerFighterEntityID, sumUnitSource, subobjSource, damageType);

            BattleUnitTid = battleUnitTid;
            FighterCfg = fighterCfg;
        }

        public override VarEnv CopyToPreVarEnv(ref VarEnv varEnv)
        {
            if (OwnerEntity == null)
            {
                BattleLogger.LogError("FighterSubobjectGenInfo CopyToPreVarEnv 异常, 未调用 Init, OwnerEntity 为空");
                return base.CopyToPreVarEnv(ref varEnv);
            }
            // if (!varEnv.HasVar<float>(CvKey.CV_SkillDmageRate))
            // {
            //     varEnv.WriteVar<float>(CvKey.CV_SkillDmageRate, 1f);
            // }
            // if (!varEnv.HasVar<EDamageType>(CvKey.CV_DamageType))
            // {
            //     varEnv.WriteVar<EDamageType>(CvKey.CV_DamageType, EDamageType.EdtNull);
            // }
            WriteVarIfAbsent(ref varEnv, CvKey.CV_BattleUnitTid, BattleUnitTid);
            if (FighterCfg != null)
                WriteVarIfAbsent(ref varEnv, CvKey.CV_FigherCfg, FighterCfg);
            return base.CopyToPreVarEnv(ref varEnv);
        }


        /// <summary>
        /// 从对象池创建并初始化战斗子物体逻辑所需的 GenInfo。
        /// </summary>
        internal static FighterSubobjectGenInfo New(
            ICustomLogicService svc,
            MetaWorld metaWorld,
            LogicEntity ownerEntity,
            long ownerFighterEntityID,
            UnitSource sumUnitSource,
            SubobjectSource subobjSource,
            int battleUnitTid,
            TFighter fighterCfg,
            TElementType damageType)
        {
            var genInfo = svc.NewGenInfo<FighterSubobjectGenInfo>();
            genInfo.Init(
                metaWorld, ownerEntity, ownerFighterEntityID,
                sumUnitSource, subobjSource,
                battleUnitTid, fighterCfg, damageType);
            return genInfo;
        }
    }
}
