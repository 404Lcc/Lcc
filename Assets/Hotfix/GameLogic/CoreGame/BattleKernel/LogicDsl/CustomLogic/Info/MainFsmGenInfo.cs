using PBConfig;

namespace LccHotfix
{
    
    
    ////////////////////////////////////////////////////////////////////////
    /// 主状态机：基础版本，场景交互物等非参战单位可用
    ////////////////////////////////////////////////////////////////////////
    public class MainFsmGenInfo : CustomLogicGenInfo, IHasLogicWorld, IHasMetaWorld, IHasOwnerEntity, IHasOwnerFighterEntityID, IHasOwnerPlayerInfo, IHasDamageType
    {
        public LogicWorld LogicWorld { get; protected set; }
        public MetaWorld MetaWorld { get; protected set; }
        public LogicEntity OwnerEntity { get; protected set; }
        public long OwnerFighterEntityID { get; protected set; }   //当前逻辑适配，晚一些移走
        public IBattlePlayerInfo OwnerPlayerInfo { get; protected set; }
        
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
            base.Destroy();
        }

        internal void Init(MetaWorld metaWorld, LogicEntity ownerEntity, TElementType damageType)
        {
            if (metaWorld == null)
                BattleLogger.LogError("MainFsmGenInfo Init 异常, MetaWorld 为空");
            if (ownerEntity == null)
                BattleLogger.LogError("MainFsmGenInfo Init 异常, OwnerEntity 为空");

            var logicWorld = ownerEntity.OwnerWorld;
            if (logicWorld == null)
                BattleLogger.LogError("MainFsmGenInfo Init 异常, LogicWorld 为空");

            var ownerFighterEntityID = ownerEntity.ID;
            if (ownerFighterEntityID == 0)
                BattleLogger.LogError("MainFsmGenInfo Init 异常, OwnerFighterEntityID 为 0");

            LogicWorld = logicWorld;
            MetaWorld = metaWorld;
            OwnerEntity = ownerEntity;
            OwnerFighterEntityID = ownerFighterEntityID;
            OwnerPlayerInfo = ownerEntity.hasComOwnerPlayer
                ? ownerEntity.comOwnerPlayer.PlayerInfoRef
                : ownerEntity.OwnerWorld?.GetCreationInfo<BattleKernelCreationInfo>()?.UnitOwnerInfoProvider?.GetOwnerInfo(ownerEntity);
            DamageType = damageType;
            if (OwnerPlayerInfo == null)
                BattleLogger.LogError("MainFsmGenInfo Init 异常, OwnerPlayerInfo 为空 (推导获取失败)");
        }

        // 黑板 key 已存在时跳过写入并打错误日志
        protected void WriteVarIfAbsent<T>(ref VarEnv varEnv, string key, T value)
        {
            if (!varEnv.HasVar<T>(key))
                varEnv.WriteVar<T>(key, value);
            else
                BattleLogger.LogError($"MainFsmGenInfo CopyToPreVarEnv 出现异常, 外部有冗余 Key={key}");
        }

        public override VarEnv CopyToPreVarEnv(ref VarEnv varEnv)
        {
            if (OwnerEntity == null)
            {
                BattleLogger.LogError("MainFsmGenInfo CopyToPreVarEnv 异常, 未调用 Init, OwnerEntity 为空");
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
        /// 从对象池创建并初始化非战斗单位主 FSM 所需的 GenInfo。
        /// </summary>
        internal static MainFsmGenInfo New(ICustomLogicService svc, MetaWorld metaWorld, LogicEntity ownerEntity, TElementType damageType)
        {
            var genInfo = svc.NewGenInfo<MainFsmGenInfo>();
            genInfo.Init(metaWorld, ownerEntity, damageType);
            return genInfo;
        }
    }
    
    
    
    ////////////////////////////////////////////////////////////////////////
    /// 主状态机：战斗单位版本
    ////////////////////////////////////////////////////////////////////////
    public class FighterMainFsmGenInfo : MainFsmGenInfo, IHasSumUnitSource, IHasFighter
    {
        private UnitSource _sumUnitSource;
        public ref UnitSource SumUnitSource => ref _sumUnitSource;

        public int BattleUnitTid { get; protected set; }
        public TFighter FighterCfg { get; protected set; }

        
        public override void Destroy()
        {
            SumUnitSource = default;
            DamageType = default;
            BattleUnitTid = 0;
            FighterCfg = null;
            base.Destroy();
        }

        internal void Init(
            MetaWorld metaWorld, LogicEntity ownerEntity,
            int battleUnitTid, TFighter fighterCfg, TElementType damageType)
        {
            base.Init(metaWorld, ownerEntity, damageType);

            var sumUnitSource = new UnitSource(ownerEntity);
            if (sumUnitSource.FighterEnityId == 0)
                BattleLogger.LogError("FighterMainFsmGenInfo Init 异常, SumUnitSource.FighterEnityId 为 0");
            if (battleUnitTid == 0)
                BattleLogger.LogError("FighterMainFsmGenInfo Init 异常, BattleUnitTid 为 0");
            if (fighterCfg == null)
                BattleLogger.LogError("FighterMainFsmGenInfo Init 异常, FighterCfg 为空");
            if (OwnerPlayerInfo == null)
                BattleLogger.LogError("FighterMainFsmGenInfo Init 异常, OwnerPlayerInfo 为空");

            SumUnitSource = sumUnitSource;
            BattleUnitTid = battleUnitTid;
            FighterCfg = fighterCfg;
        }

        public override VarEnv CopyToPreVarEnv(ref VarEnv varEnv)
        {
            if (OwnerEntity == null)
            {
                BattleLogger.LogError("FighterMainFsmGenInfo CopyToPreVarEnv 异常, 未调用 Init, OwnerEntity 为空");
                return base.CopyToPreVarEnv(ref varEnv);
            }

            WriteVarIfAbsent(ref varEnv, CvKey.CV_BattleUnitTid, BattleUnitTid);
            WriteVarIfAbsent(ref varEnv, CvKey.CV_FigherCfg, FighterCfg);
            return base.CopyToPreVarEnv(ref varEnv);
        }

        /// <summary>
        /// 从对象池创建并初始化战斗单位主 FSM 所需的 GenInfo。
        /// </summary>
        internal static FighterMainFsmGenInfo New(
            ICustomLogicService svc,
            MetaWorld metaWorld,
            LogicEntity ownerEntity,
            int battleUnitTid,
            TFighter fighterCfg,
            TElementType damageType)
        {
            var genInfo = svc.NewGenInfo<FighterMainFsmGenInfo>();
            genInfo.Init(metaWorld, ownerEntity, battleUnitTid, fighterCfg, damageType);
            return genInfo;
        }
    }
}
