using System.Runtime.CompilerServices;
using PBConfig;

namespace LccHotfix
{
    public partial class VarEnv
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void FillBuffSourceVarEnv(LogicEntity fromEntity)
        {
            WriteVar<long>(CvKey.CV_OwnerFighterEntityID, fromEntity.ID);

            var playerInfo = fromEntity.OwnerWorld?.GetCreationInfo<BattleKernelCreationInfo>()?.UnitOwnerInfoProvider?.GetOwnerInfo(fromEntity);
            if (playerInfo != null)
            {
                WriteVar(CvKey.CV_OwnerPlayerInfo, playerInfo);
            }

            if (fromEntity.hasComBattleUnitTag)
            {
                var battleUnitTID = fromEntity.comBattleUnitTag.Tag.BattleUnitTid ?? 0;
                WriteVar(CvKey.CV_BattleUnitTid, battleUnitTID);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void FillBuffSourceVarEnv(VarEnv newEnv)
        {
            // 这里表示 Buff 的来源实体信息，暂时仍复用旧变量名。
            CopyTo<long>(newEnv, CvKey.CV_OwnerFighterEntityID);
            CopyTo<IBattlePlayerInfo>(newEnv, CvKey.CV_OwnerPlayerInfo);
            CopyTo<int>(newEnv, CvKey.CV_BattleUnitTid);
            CopyTo<TFighter>(newEnv, CvKey.CV_FigherCfg);
            CopyTo<float>(newEnv, CvKey.CV_SkillDmageRate, false);
            CopyTo<EDamageType>(newEnv, CvKey.CV_DamageType, false);
        }
    }
}
