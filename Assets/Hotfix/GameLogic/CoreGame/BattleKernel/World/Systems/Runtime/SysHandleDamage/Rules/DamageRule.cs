using System;

namespace LccHotfix
{
    // 伤害计算器
    public class AdvancedDamageCalculator : IDamageCalculator
    {
        private readonly Random _random = new Random();
        
        public DamageResult Calculate(ref DamageContext context)
        {
            var result = DamageResult.Default;

            // 1. 命中判定
            if (!CheckHit(context))
            {
                result.IsMiss = true;
                return result;
            }
            
            // 2. 秒杀判定
            if (CheckInstantKill(context))
            {
                result.IsInstantKill = true;
                result.FinalDamage = CalculateInstantKillDamage(context);
                return result;
            }
            
            // 3. 计算基础伤害
            double baseDamage = CalculateBaseDamage(ref context);

            // 4. 伤害类型减免
            baseDamage = ApplyDamageTypeDamage(context, baseDamage);
            
            // 5. 暴击判定和计算
            bool isCritical = CheckCritical(context);
            if (isCritical)
            {
                result.IsCritical = true;
                baseDamage = ApplyCriticalDamage(baseDamage, context);
            }
            
            // 6. 随机波动
            baseDamage *= (1 + context.RandomFinalDamageRate + (context.Attacker.Properties.FinalAtk / 10000f));
            if (BattleLog.IsDebugEnabled)
                BattleLog.Debug($"[伤害计算 6 随机伤害] 最终伤害：{baseDamage} 随机波动：{context.RandomFinalDamageRate}， 点数修正最终伤害{context.Attacker.Properties.FinalAtk / 10000}");
            result.FinalDamage = baseDamage;
            return result;
        }
        
        private bool CheckHit(in DamageContext context)
        {
            double hitRate = CalculateHitRate(context);
            return context.Random.NextDouble() <= hitRate;
        }
        
        private double CalculateHitRate(in DamageContext context)
        {
            double hit = context.Attacker.Properties.Hit;
            double dodge = context.Defender.Properties.Dodge;
            double hitRate = (hit - dodge) / 10000.0;
            return Math.Clamp(hitRate, 0.2, 1.0);
        }
        
        private bool CheckInstantKill(in DamageContext context)
        {
            if (context.Defender.Properties.InstantKillImmune)
                return false;
            double instantKillRate = CalculateInstantKillRate(context);
            return context.Random.NextDouble() <= instantKillRate;
        }
        
        private double CalculateInstantKillRate(in DamageContext context)
        {
            double instantKill = context.Attacker.Properties.InstantKill;
            double instantKillResist = context.Defender.Properties.InstantKillResist;
            double rate = (instantKill - instantKillResist) / 10000.0;
            return Math.Clamp(rate, 0.0, 1.0);
        }
        
        private double CalculateInstantKillDamage(in DamageContext context)
        {
            // 秒杀伤害逻辑 - 造成伤害时X%概率秒杀目标（对精英和首领无效）（单次最大伤害为攻击力的4000X倍）
            var atk = context.Attacker.Properties.Attack;
            var dmg = context.Defender.Properties.Health;
            double instantKill = context.Attacker.Properties.InstantKill;
            var maxDmg = atk * 4000 * instantKill * 100;
            dmg = dmg > maxDmg ? maxDmg : dmg;
            return dmg;
        }
        
        private bool CheckCritical(in DamageContext context)
        {
            double critRate = CalculateCritRate(context);
            return context.Random.NextDouble() <= critRate;
        }
        
        private double CalculateCritRate(in DamageContext context)
        {
            double crit = context.Attacker.Properties.Crit;
            double critResist = context.Defender.Properties.CritResist;
            double rate = (crit - critResist) / 10000.0;
            return Math.Clamp(rate, 0.0, 1.0);
        }
        
        private double CalculateBaseDamage(ref DamageContext context)
        {
            // 按照公式计算：((A攻击*A攻击/(A攻击+B防御))*A技能伤害系数+A技能固定值修正)
            // *(1+A局外增伤-B局外伤害抗性)*(1+A局内增伤-B局内伤害抗性)*(1+关卡伤害修正)
            
            // 攻击力计算单元
            var attackCalculator = new AttackCalculCell(context.Attacker, context.Defender);
            double attack = attackCalculator.CalculateTotalAttack();
            double defense = context.Defender.Properties.Defense * (1 + context.Defender.Properties.ScaleDef / 10000f);
            var subobjectTid = context.Subobject.GetValueOrDefault().SubobjectTid;
            
            // 应用破甲
            double armorPierce = context.Attacker.Properties.ArmorPierce / 10000.0;
            armorPierce = Math.Clamp(armorPierce, 0.0, 1.0);
            defense *= (1.0 - armorPierce);
            
            // 基准攻击防御部分
            double attackDefensePart = (attack * attack) / (attack + defense);
            
            // 技能修正
            double skillPart = attackDefensePart * context.SkillDamageFactor + context.SkillFixedDamage;
            if(BattleLog.IsDebugEnabled)
                BattleLog.Debug($"[子弹:{subobjectTid}] attack={attack}, defense={defense}, skillPart({skillPart}) = attackDefensePart({attackDefensePart}) * SkillDamageFactor({context.SkillDamageFactor}) + SkillFixedDamage({context.SkillFixedDamage})");
            
            // 局外增伤抗性
            double outgameAmplify = context.Attacker.Properties.DamageAmplify / 10000.0;
            double outgameResist = context.Defender.Properties.DamageResistance / 10000.0;
            double outgameFactor = 1.0 + outgameAmplify - outgameResist;
            outgameFactor = Math.Max(outgameFactor, 0.2);
            
            // 局内增伤抗性
            double ingameAmplify = context.Attacker.Properties.InGameAmplify / 10000.0;
            double ingameResist = context.Defender.Properties.InGameResistance / 10000.0;
            double ingameFactor = 1.0 + ingameAmplify - ingameResist;
            ingameFactor = Math.Max(ingameFactor, 0.2);
            
            // 关卡修正
            double stageFactor = 1.0 + context.StageDamageFactor;

            // 最终加法修正
            double finalAddPart = context.FinalFixedDamage - context.FinalFixedReduceDamage;
            
            // 最终伤害
            double finalDamage = skillPart * outgameFactor * ingameFactor * stageFactor + finalAddPart;
            
            if(BattleLog.IsDebugEnabled)
                BattleLog.Debug($"[子弹:{subobjectTid}] finalDamage({finalDamage}) = skillPart({skillPart}) * outgameFactor({outgameFactor}) * ingameFactor({ingameFactor}) * stageFactor({stageFactor}) + finalAddPart({finalAddPart})");
            return Math.Max(0.0, finalDamage);
        }
        
        private double ApplyCriticalDamage(double baseDamage, in DamageContext context)
        {
            double critDamageBonus = context.Attacker.Properties.CritDamage / 10000.0;
            var result =  baseDamage * (1.0 + critDamageBonus) + context.ExtraCritDamage;
            if(BattleLog.IsDebugEnabled)
                BattleLog.Debug($"暴击 ApplyCriticalDamage result:{result} = baseDamage:{baseDamage} * (1.0 + critDamageBonus:{critDamageBonus}) + ExtraCritDamage:{context.ExtraCritDamage}");
            return result;
        }

        private double ApplyDamageTypeDamage(in DamageContext context, double baseDamage)
        {
            return context.World?.DamagePropertyModifier?.ApplyDamageTypeDamage(context, baseDamage) ?? baseDamage;
        }
    }
}
