
namespace LccHotfix
{
    ////////////////////////////////////////////////////////////////////////
    /// 玩家补给逻辑：归属玩家、由 System 持有的匿名 CustomLogic 初始化规格
    ////////////////////////////////////////////////////////////////////////
    /// <summary>
    /// 玩家补给逻辑运行时初始化信息：世界上下文、归属玩家与补给参数。
    /// </summary>
    public class SupplyLogicGenInfo : CustomLogicGenInfo, IHasLogicWorld, IHasMetaWorld, IHasOwnerPlayerInfo
    {
        // 逻辑世界引用
        public LogicWorld LogicWorld { get; protected set; }
        // 元世界引用
        public MetaWorld MetaWorld { get; protected set; }
        // 归属玩家信息
        public IBattlePlayerInfo OwnerPlayerInfo { get; protected set; }
        // 补给逻辑配置参数（含义由具体 LogicConfig 约定）
        public int SupplyLogicParams { get; protected set; }
        public uint BattleSupplyId { get; set; }

        public override void Destroy()
        {
            LogicWorld = null;
            MetaWorld = null;
            OwnerPlayerInfo = null;
            SupplyLogicParams = 0;
            BattleSupplyId = 0;
            base.Destroy();
        }

        internal void Init(
            LogicWorld logicWorld,
            MetaWorld metaWorld,
            IBattlePlayerInfo ownerPlayerInfo,
            int supplyLogicParams)
        {
            if (logicWorld == null)
                BattleLogger.LogError("SupplyLogicGenInfo Init 异常, LogicWorld 为空");
            if (metaWorld == null)
                BattleLogger.LogError("SupplyLogicGenInfo Init 异常, MetaWorld 为空");
            if (ownerPlayerInfo == null)
                BattleLogger.LogError("SupplyLogicGenInfo Init 异常, OwnerPlayerInfo 为空");

            LogicWorld = logicWorld;
            MetaWorld = metaWorld;
            OwnerPlayerInfo = ownerPlayerInfo;
            SupplyLogicParams = supplyLogicParams;
        }

        // 黑板 key 已存在时跳过写入并打错误日志
        protected void WriteVarIfAbsent<T>(ref VarEnv varEnv, string key, T value)
        {
            if (!varEnv.HasVar<T>(key))
                varEnv.WriteVar<T>(key, value);
            else
                BattleLogger.LogError($"SupplyLogicGenInfo CopyToPreVarEnv 出现异常, 外部有冗余 Key={key}");
        }

        public override VarEnv CopyToPreVarEnv(ref VarEnv varEnv)
        {
            if (OwnerPlayerInfo == null)
            {
                BattleLogger.LogError("SupplyLogicGenInfo CopyToPreVarEnv 异常, 未调用 Init, OwnerPlayerInfo 为空");
                return base.CopyToPreVarEnv(ref varEnv);
            }

            WriteVarIfAbsent(ref varEnv, CvKey.CV_LogicWorld, LogicWorld);
            WriteVarIfAbsent(ref varEnv, CvKey.CV_MetaWorld, MetaWorld);
            WriteVarIfAbsent(ref varEnv, CvKey.CV_OwnerPlayerInfo, OwnerPlayerInfo);
            WriteVarIfAbsent(ref varEnv, CvKey.CV_SupplyLogicParams, SupplyLogicParams);
            WriteVarIfAbsent(ref varEnv, CvKey.CV_BattleSupplyId, BattleSupplyId);
            return base.CopyToPreVarEnv(ref varEnv);
        }

        /// <summary>
        /// 从对象池创建并初始化玩家补给逻辑所需的 GenInfo。
        /// </summary>
        internal static SupplyLogicGenInfo New(
            ICustomLogicService svc,
            LogicWorld logicWorld,
            MetaWorld metaWorld,
            IBattlePlayerInfo ownerPlayerInfo,
            int supplyLogicParams)
        {
            var genInfo = svc.NewGenInfo<SupplyLogicGenInfo>();
            genInfo.Init(logicWorld, metaWorld, ownerPlayerInfo, supplyLogicParams);
            return genInfo;
        }
    }
}
