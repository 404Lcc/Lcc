namespace LccHotfix
{
    /// <summary>
    /// LogicEntity Buff 生成信息创建相关扩展。
    /// </summary>
    public static class LogicEntityBuffExtensions
    {
        /// <summary>
        /// 为目标实体创建 BuffGenInfo，并写入 Buff 逻辑容器、等级和基础黑板变量。
        /// </summary>
        public static BuffGenInfo CreateBuffGenInfo(this LogicEntity entity, int buffLogicId, int maxLvl = 1)
        {
            var svc = entity?.OwnerWorld?.GetCreationInfo<BattleKernelCreationInfo>()?.CustomLogicService;
            if (svc == null)
            {
                BattleLogger.LogError("CreateBuffGenInfo CustomLogicService == null");
                return null;
            }

            var varEnv = svc.NewVarEnv();
            varEnv.WriteVar(CvKey.CV_OwnerEntity, entity);
            varEnv.WriteVar(CvKey.CV_LogicWorld, entity.OwnerWorld);

            var genInfo = svc.NewGenInfo<BuffGenInfo>();
            genInfo.BuffLevel = 1;
            genInfo.BuffMaxLevel = maxLvl != 0 ? maxLvl : 1;
            genInfo.Owner = entity;
            genInfo.LogicConfigID = buffLogicId;
            genInfo.ConfigContainerName = LogicContainerKey.LogicConfigs_Buff;
            genInfo.PreEnv = varEnv;
            return genInfo;
        }
    }
}
