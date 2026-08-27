using System;
using System.Runtime.CompilerServices;
using PBConfig;
using UnityEngine;

namespace LccHotfix
{

    
    //////////////////////////////////////////////////////////////////////////
    /// 这里集中整理的是：当前项目CustomLogic，Node中最基础的黑板约定
    /// 这里集中整理的是：当前项目CustomLogic，Node中最基础的黑板约定
    /// 这里集中整理的是：当前项目CustomLogic，Node中最基础的黑板约定
    /// 最基础的黑板规格约定 ！！！！
    /// eg:
    /// 技能逻辑黑板中 约好一定有 XXXX
    /// 子物体逻辑黑板中 约好一定有 XXXX
    /// 
    public static class CustomNodeVarEnvFillExtensions
    {
        /// <summary>
        /// 填充技能逻辑所需的基础黑板变量。
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void FillSkillBaseVarEnv(this CustomNode self, VarEnv newEnv, LogicEntity skill_e, TSkillLogic skillCfg)
        {
            var env = self.VarEnvRef; //mainFSMNode、aiNode

            var logicID = skillCfg.LogicID;
            var damageRate = skillCfg.DamageRate;
            var subTids = skillCfg.SubobjIds;
            var subTid = subTids?.Count > 0 ? (int)subTids[0] : -1;
            var skillTid = skillCfg.Base.Id;
            newEnv.WriteVar<float>(CvKey.CV_SearchRange, skillCfg.Range);
            newEnv.WriteVar(CvKey.CV_SkillTid, (int)skillTid);
            newEnv.WriteVar(CvKey.CV_SkillDmageRate, damageRate);
            newEnv.WriteVar(CvKey.CV_BaseSpawnSbjTid, subTid);
            newEnv.WriteVar(CvKey.CV_SpawnSbjTid, subTid);
            if (self.IsDev())
            {
                BattleLogger.LogDebug($"FillSkillBaseVarEnv damageRate={damageRate}, subTid={subTid}, skillTid={skillTid}, logicID={logicID}");
            }

            env.CopyTo<int>(newEnv, CvKey.CV_SkillLevel, false);
            env.CopyTo<Vector3>(newEnv, CvKey.CV_TargetPos, false);
        }

        /// <summary>
        /// 填充子物体逻辑所需的基础黑板变量。
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void FillSubobjectBaseVarEnv(this CustomNode self, VarEnv newEnv, LogicEntity e)
        {
            var env = self.VarEnvRef; //skillNode、subobjectNode
            env.CopyTo<int>(newEnv, CvKey.CV_SkillTid, false);        //容许为0
            env.CopyTo<int>(newEnv, CvKey.CV_SkillLevel, false);      //容许为0
            env.CopyTo<uint>(newEnv, CvKey.CV_BattleSupplyId, false);
            env.CopyTo<float>(newEnv, CvKey.CV_SkillDmageRate, false);
            env.CopyTo<EDamageType>(newEnv, CvKey.CV_DamageType, false);
            env.CopyTo<float>(newEnv, CvKey.CV_SearchRange, false);
            env.CopyTo<string>(newEnv, CvKey.CV_SbjHitFxResOverride, false);
            env.CopyTo<float>(newEnv, CvKey.CV_AmmoGambleCritAdd, false);
            env.CopyTo<float>(newEnv, CvKey.CV_AmmoGambleCritDamageAdd, false);
            env.CopyTo<float>(newEnv, CvKey.CV_AmmoGambleDamageMultiplier, false);
            env.CopyTo<bool>(newEnv, CvKey.CV_AmmoGambleForceCritical, false);
        }

        /// <summary>
        /// 填充 Buff 逻辑所需的基础黑板变量；当前仍沿用“宿主/来源”混用字段，后续继续收口。
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void FillBuffBaseVarEnv(this CustomNode self, VarEnv newEnv, LogicEntity e)
        {
            var env = self.VarEnvRef; //skillNode、subobjectNode
            newEnv.WriteVar<LogicEntity>(CvKey.CV_OwnerEntity, e);
            env.CopyTo<LogicWorld>(newEnv, CvKey.CV_LogicWorld);
            env.CopyTo<MetaWorld>(newEnv, CvKey.CV_MetaWorld);

            // 这里表示 Buff 的来源实体信息，暂时仍复用旧变量名。
            env.CopyTo<long>(newEnv, CvKey.CV_OwnerFighterEntityID);
            env.CopyTo<IBattlePlayerInfo>(newEnv, CvKey.CV_OwnerPlayerInfo);
            env.CopyTo<int>(newEnv, CvKey.CV_BattleUnitTid);
            env.CopyTo<TFighter>(newEnv, CvKey.CV_FigherCfg);
            env.CopyTo<float>(newEnv, CvKey.CV_SkillDmageRate, false);
            env.CopyTo<EDamageType>(newEnv, CvKey.CV_DamageType, false);
        }
    }
}
