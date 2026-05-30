using PBConfig;

namespace LccHotfix
{
    /// <summary>
    /// LogicEntity 写入 CustomLogic 黑板上下文的扩展。
    /// </summary>
    public static class LogicEntityBlackboardExtensions
    {
        /// <summary>
        /// 填充实体主 FSM 所需的基础黑板上下文。
        /// </summary>
        public static void FillMainFsmVarEnv(this LogicEntity entity, VarEnv newEnv, MetaWorld metaWorld, int battleUnitTid, TFighter fighterCfg)
        {
            newEnv.WriteVar(CvKey.CV_OwnerEntity, entity);
            newEnv.WriteVar(CvKey.CV_LogicWorld, entity.OwnerWorld);
            newEnv.WriteVar(CvKey.CV_MetaWorld, metaWorld);
            newEnv.WriteVar(CvKey.CV_OwnerFighterEntityID, entity.ID);
            newEnv.WriteVar(CvKey.CV_BattleUnitTid, battleUnitTid);
            newEnv.WriteVar(CvKey.CV_FigherCfg, fighterCfg);
        }
    }
}
