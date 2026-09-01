using PBConfig;

namespace LccHotfix
{
    ////////////////////////////////////////////////////////////////////////
    /// 技能逻辑：基础版本
    ////////////////////////////////////////////////////////////////////////
    public class SkillLogicGenInfo : CustomLogicGenInfo,
        IHasLogicWorld,
        IHasMetaWorld,
        IHasOwnerEntity,
        IHasOwnerFighterEntityID,
        IHasOwnerPlayerInfo,
        IHasSumUnitSource,
        IHasDamageType
    {
        private UnitSource _sumUnitSource;

        public LogicWorld LogicWorld { get; protected set; }
        public MetaWorld MetaWorld { get; protected set; }
        public LogicEntity OwnerEntity { get; protected set; }
        public long OwnerFighterEntityID { get; protected set; }
        public InGamePlayerInfo OwnerPlayerInfo { get; protected set; }
        public ref UnitSource SumUnitSource => ref _sumUnitSource;
        private TElementType _damageType;
        public ref TElementType DamageType
        {
            get { return ref _damageType; }
        }

        public SkillLogicGenInfo()
        {
        }

        internal void Init(
            MetaWorld metaWorld,
            LogicEntity ownerEntity,
            long ownerFighterEntityID,
            UnitSource sumUnitSource,
            TElementType damageType)
        {
            if (metaWorld == null)
                KLogger.LogError("SkillLogicGenInfo Init 异常, MetaWorld 为空");
            if (ownerEntity == null)
                KLogger.LogError("SkillLogicGenInfo Init 异常, OwnerEntity 为空");

            var logicWorld = ownerEntity.OwnerWorld;
            if (logicWorld == null)
                KLogger.LogError("SkillLogicGenInfo Init 异常, LogicWorld 为空");
            if (ownerFighterEntityID == 0)
                KLogger.LogError("SkillLogicGenInfo Init 异常, OwnerFighterEntityID 为 0");

            LogicWorld = logicWorld;
            MetaWorld = metaWorld;
            OwnerEntity = ownerEntity;
            OwnerFighterEntityID = ownerFighterEntityID;
            OwnerPlayerInfo = ownerEntity.GetPlayerInfo();
            SumUnitSource = sumUnitSource;
            DamageType = damageType;

            if (OwnerPlayerInfo == null)
                KLogger.LogError("SkillLogicGenInfo Init 异常, OwnerPlayerInfo 为空");
        }

        // 黑板 key 已存在时跳过写入并打错误日志
        protected void WriteVarIfAbsent<T>(ref VarEnv varEnv, string key, T value)
        {
            if (!varEnv.HasVar<T>(key))
                varEnv.WriteVar<T>(key, value);
            else
                KLogger.LogError($"SkillLogicGenInfo CopyToPreVarEnv 出现异常, 外部有冗余 Key={key}");
        }

        public override VarEnv CopyToPreVarEnv(ref VarEnv varEnv)
        {
            if (OwnerEntity == null)
            {
                KLogger.LogError("SkillLogicGenInfo CopyToPreVarEnv 异常, 未调用 Init, OwnerEntity 为空");
                return base.CopyToPreVarEnv(ref varEnv);
            }
            WriteVarIfAbsent(ref varEnv, CvKey.CV_LogicWorld, LogicWorld);
            WriteVarIfAbsent(ref varEnv, CvKey.CV_MetaWorld, MetaWorld);
            WriteVarIfAbsent(ref varEnv, CvKey.CV_OwnerEntity, OwnerEntity);
            WriteVarIfAbsent(ref varEnv, CvKey.CV_OwnerFighterEntityID, OwnerFighterEntityID);
            WriteVarIfAbsent(ref varEnv, CvKey.CV_OwnerPlayerInfo, OwnerPlayerInfo);
            return base.CopyToPreVarEnv(ref varEnv);
        }

        public override void Destroy()
        {
            LogicWorld = null;
            MetaWorld = null;
            OwnerEntity = null;
            OwnerFighterEntityID = 0;
            OwnerPlayerInfo = null;
            SumUnitSource = default;
            base.Destroy();
        }

        /// <summary>
        /// 从对象池创建并初始化技能逻辑所需的 GenInfo。
        /// </summary>
        internal static SkillLogicGenInfo New(
            ICustomLogicService svc,
            MetaWorld metaWorld,
            LogicEntity ownerEntity,
            long ownerFighterEntityID,
            UnitSource sumUnitSource,
            TElementType damageType)
        {
            var genInfo = svc.NewGenInfo<SkillLogicGenInfo>();
            genInfo.Init(metaWorld, ownerEntity, ownerFighterEntityID, sumUnitSource, damageType);
            return genInfo;
        }

        /// <summary>
        /// 从主状态机 GenInfo 创建技能逻辑 GenInfo，自动区分战斗/非战斗单位。
        /// </summary>
        internal static SkillLogicGenInfo FromMainFsm(
            ICustomLogicService svc,
            MainFsmGenInfo mainFsm)
        {
            if (mainFsm is FighterMainFsmGenInfo fighterMainFsm)
                return FighterSkillLogicGenInfo.FromMainFsm(svc, fighterMainFsm);
            
            return New(
                svc,
                mainFsm.MetaWorld,
                mainFsm.OwnerEntity,
                mainFsm.OwnerFighterEntityID,
                new UnitSource(mainFsm.OwnerEntity),
                mainFsm.DamageType);
        }
    }



    ////////////////////////////////////////////////////////////////////////
    /// 技能逻辑：战斗单位版本
    ////////////////////////////////////////////////////////////////////////
    public class FighterSkillLogicGenInfo : SkillLogicGenInfo, IHasFighter
    {
        public int BattleUnitTid { get; protected set; }
        public TFighter FighterCfg { get; protected set; }

        public FighterSkillLogicGenInfo()
        {
        }

        internal void Init(
            MetaWorld metaWorld,
            LogicEntity ownerEntity,
            long ownerFighterEntityID,
            UnitSource sumUnitSource,
            int battleUnitTid,
            TFighter fighterCfg,
            TElementType damageType)
        {
            base.Init(metaWorld, ownerEntity, ownerFighterEntityID, sumUnitSource, damageType);

            BattleUnitTid = battleUnitTid;
            FighterCfg = fighterCfg;
        }

        public override VarEnv CopyToPreVarEnv(ref VarEnv varEnv)
        {
            if (OwnerEntity == null)
            {
                KLogger.LogError("FighterSkillLogicGenInfo CopyToPreVarEnv 异常, 未调用 Init, OwnerEntity 为空");
                return base.CopyToPreVarEnv(ref varEnv);
            }
            WriteVarIfAbsent(ref varEnv, CvKey.CV_BattleUnitTid, BattleUnitTid);
            if (FighterCfg != null)
                WriteVarIfAbsent(ref varEnv, CvKey.CV_FigherCfg, FighterCfg);
            return base.CopyToPreVarEnv(ref varEnv);
        }

        public override void Destroy()
        {
            BattleUnitTid = 0;
            FighterCfg = null;
            base.Destroy();
        }

        /// <summary>
        /// 从战斗单位主状态机 GenInfo 创建技能逻辑 GenInfo。
        /// </summary>
        internal static FighterSkillLogicGenInfo FromMainFsm(
            ICustomLogicService svc,
            FighterMainFsmGenInfo mainFsm)
        {
            return New(
                svc,
                mainFsm.MetaWorld,
                mainFsm.OwnerEntity,
                mainFsm.OwnerFighterEntityID,
                mainFsm.SumUnitSource,
                mainFsm.BattleUnitTid,
                mainFsm.FighterCfg,
                mainFsm.DamageType);
        }

        /// <summary>
        /// 从对象池创建并初始化战斗单位技能逻辑所需的 GenInfo。
        /// </summary>
        internal static FighterSkillLogicGenInfo New(
            ICustomLogicService svc,
            MetaWorld metaWorld,
            LogicEntity ownerEntity,
            long ownerFighterEntityID,
            UnitSource sumUnitSource,
            int battleUnitTid,
            TFighter fighterCfg,
            TElementType damageType)
        {
            var genInfo = svc.NewGenInfo<FighterSkillLogicGenInfo>();
            genInfo.Init(
                metaWorld, ownerEntity, ownerFighterEntityID,
                sumUnitSource, battleUnitTid, fighterCfg, damageType);
            return genInfo;
        }
    }
}
