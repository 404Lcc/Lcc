using PBConfig;

namespace LccHotfix
{
    public static class CustomNodeSkillExtensions
    {
    #region 技能创建


        /// <summary>
        /// 创建技能流程黑板，写入目标、伤害类型和攻击次数后挂载技能流程。
        /// </summary>
        public static bool CreateSkillProcess(this CustomNode self, LogicEntity entity, int skillTid, LogicEntity target,
            EDamageType damageType, int curAttackTimes, float attackInterval = -1f)
        {
            var varEnv = self.GetLogicWorld().GetCreationInfo<BattleKernelCreationInfo>().CustomLogicService.NewVarEnv();
            varEnv.WriteVar(CvKey.CV_TargetEid, target.ID);
            varEnv.WriteVar(CvKey.CV_TargetPos, target.GetMainViewBindPos("Hit"));
            varEnv.WriteVar(CvKey.CV_DamageType, damageType);
            varEnv.WriteVar(CvKey.CV_CurAttackTimes, curAttackTimes);
            if (attackInterval > 0f)
                varEnv.WriteVar(CvKey.CV_AttackInterval, attackInterval);
            return self.CreateSkillProcess(entity, skillTid, varEnv);
        }

        /// <summary>
        /// 使用已有黑板创建并挂载技能流程，同时向实体发送技能命令。
        /// </summary>
        public static bool CreateSkillProcess(this CustomNode self, LogicEntity entity, int skillTid, VarEnv varEnv)
        {
            if (varEnv == null)
                varEnv = self.GetLogicWorld().GetCreationInfo<BattleKernelCreationInfo>().CustomLogicService.NewVarEnv();

            var skillProcess = self.CreateSkillProcessLogic(entity, skillTid, varEnv);
            if (skillProcess == null)
            {
                return false;
            }

            var cmd = new EntityCommand { CmdType = EntityCmdType.Nt_Skill };
            entity.SendCmd(cmd);
            entity.ReplaceComSkillProcess((uint)skillTid, skillProcess);
            return true;
        }

        /// <summary>
        /// 根据技能配置创建技能逻辑实例，不直接挂载到实体技能流程组件。
        /// </summary>
        public static SkillLogic CreateSkillProcessLogic(this CustomNode self, LogicEntity entity, int skillTid, VarEnv varEnv = null)
        {
            var skillCfg = PbCfg.GetData<TSkillLogic>((uint)skillTid);
            var logicID = skillCfg.LogicID;
            logicID = ResolveSkillLogicID(self, logicID);

            var svc = self.GetLogicWorld().GetCreationInfo<BattleKernelCreationInfo>().CustomLogicService;
            if (varEnv == null)
            {
                varEnv = svc.NewVarEnv();
            }

            self.FillSkillBaseVarEnv(varEnv, entity, skillCfg);

            var mainFsmGenInfo = self.GetGenInfo<MainFsmGenInfo>();
            var genInfo = SkillLogicGenInfo.FromMainFsm(svc, mainFsmGenInfo);
            genInfo.LogicConfigID = logicID;
            genInfo.ConfigContainerName = LogicContainerKey.LogicConfigs_Skill;
            genInfo.PreEnv = varEnv;
            return svc.CreateLogic<SkillLogic>(genInfo);
        }

        /// <summary>
        /// 根据外部技能逻辑重写服务修正技能LogicID。
        /// </summary>
        private static int ResolveSkillLogicID(CustomNode self, int logicID)
        {
            var world = self.GetLogicWorld();
            var player = self.GetOwnerBattlePlayerInfo();
            var fighterCfg = self.GetVar<TFighter>(CvKey.CV_FigherCfg);
            var newLogicID = world?.GetCreationInfo<BattleKernelCreationInfo>()?.SkillLogicOverrideProvider?.ResolveSkillLogicId(player, fighterCfg, logicID) ?? logicID;
            if (newLogicID != logicID)
            {
                CLogger.LogInfo(self, $"ResolveSkillLogicID 特性修正技能ID logicID:{logicID} -> {newLogicID}");
            }

            return newLogicID;
        }

    #endregion
    }
}
