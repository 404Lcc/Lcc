using System;

namespace LccHotfix
{
    public static class PropertyFloat
    {
        public const int Health = 1;
        public const int Attack = 2;
        public const int Defense = 3;
        public const int MoveSpeed = 6;
        public const int MoveSpeedRatio = 7;
        public const int BulletTimeRatio = 8;
        public const int AnimSpeedRatio = 9;

        public const int Hit = 10;
        public const int Dodge = 11;

        public const int Crit = 12;
        public const int CritResist = 13;
        public const int CritDamage = 14;

        public const int InstantKill = 15;
        public const int InstantKillResist = 16;

        public const int ArmorPierce = 17;

        public const int DamageAmplify = 18;
        public const int DamageResistance = 19;
        public const int InGameAmplify = 20;
        public const int InGameResistance = 21;

        public const int Tenacity = 22;

        public const int VsBossAmplify = 23;

        public const int ScaleHpBase = 24;
        public const int ScaleAtkBase = 25;
        public const int ScaleHp = 26;
        public const int ScaleAtk = 27;
        public const int ScaleDef = 28;
        public const int FinalAtk = 29;
    }

    // 属性快照切片。
    [Serializable]
    public struct PropertySnapshot
    {
        public int VersionCode;

        public double Health;
        public double Attack;
        public double Defense;

        public double Hit;
        public double Dodge;

        public double Crit;
        public double CritResist;
        public double CritDamage;

        public double InstantKill;
        public double InstantKillResist;
        public bool InstantKillImmune;

        public double ArmorPierce;

        public double DamageAmplify;
        public double DamageResistance;
        public double InGameAmplify;
        public double InGameResistance;

        public double Tenacity;

        public double VsBossAmplify;

        public double ScaleHpBase;
        public double ScaleAtkBase;
        public double ScaleHp;
        public double ScaleAtk;
        public double ScaleDef;

        public double FinalAtk;

        public UnitSkillFeature Feature;

        public bool AddPropertyByKey(int key, double valueAdd)
        {
            if (BattleLogger.IsDebugEnabled)
                BattleLogger.LogDebug($"SetPropertyByAttributeKey key={key}, valueAdd={valueAdd}");

            switch (key)
            {
                case PropertyFloat.Health:
                    Health += valueAdd;
                    break;
                case PropertyFloat.Attack:
                    Attack += valueAdd;
                    break;
                case PropertyFloat.Defense:
                    Defense += valueAdd;
                    break;
                case PropertyFloat.Hit:
                    Hit += valueAdd;
                    break;
                case PropertyFloat.Dodge:
                    Dodge += valueAdd;
                    break;
                case PropertyFloat.Crit:
                    Crit += valueAdd;
                    break;
                case PropertyFloat.CritResist:
                    CritResist += valueAdd;
                    break;
                case PropertyFloat.CritDamage:
                    CritDamage += valueAdd;
                    break;
                case PropertyFloat.InstantKill:
                    InstantKill += valueAdd;
                    break;
                case PropertyFloat.InstantKillResist:
                    InstantKillResist += valueAdd;
                    break;
                case PropertyFloat.ArmorPierce:
                    ArmorPierce += valueAdd;
                    break;
                case PropertyFloat.DamageAmplify:
                    DamageAmplify += valueAdd;
                    break;
                case PropertyFloat.DamageResistance:
                    DamageResistance += valueAdd;
                    break;
                case PropertyFloat.InGameAmplify:
                    InGameAmplify += valueAdd;
                    break;
                case PropertyFloat.InGameResistance:
                    InGameResistance += valueAdd;
                    break;
                case PropertyFloat.Tenacity:
                    Tenacity += valueAdd;
                    break;
                case PropertyFloat.ScaleAtkBase:
                    ScaleAtkBase += valueAdd;
                    break;
                case PropertyFloat.ScaleHpBase:
                    ScaleHpBase += valueAdd;
                    break;
                case PropertyFloat.ScaleDef:
                    ScaleDef += valueAdd;
                    break;
                case PropertyFloat.FinalAtk:
                    FinalAtk += valueAdd;
                    break;
                default:
                    BattleLogger.LogError($"SetPropertyByAttributeKey 无效的 AttributeKey={key}");
                    return false;
            }

            return true;
        }

        public bool FillFromEntity(LogicEntity entity)
        {
            if (entity == null)
                return false;
            if (!entity.hasComAttributes)
                return false;

            var comAttributes = entity.comAttributes;
            comAttributes.TryGetValue<double>(PropertyFloat.Health, out Health, 300);
            comAttributes.TryGetValue<double>(PropertyFloat.Attack, out Attack, 100);
            comAttributes.TryGetValue<double>(PropertyFloat.Defense, out Defense, 100);
            comAttributes.TryGetValue<double>(PropertyFloat.Hit, out Hit, 10000f);
            comAttributes.TryGetValue<double>(PropertyFloat.Dodge, out Dodge, 0f);

            comAttributes.TryGetValue<double>(PropertyFloat.Crit, out Crit, 0f);
            comAttributes.TryGetValue<double>(PropertyFloat.CritResist, out CritResist, 0f);
            comAttributes.TryGetValue<double>(PropertyFloat.CritDamage, out CritDamage, 0f);

            comAttributes.TryGetValue<double>(PropertyFloat.InstantKill, out InstantKill, 0f);
            comAttributes.TryGetValue<double>(PropertyFloat.InstantKillResist, out InstantKillResist, 0f);

            comAttributes.TryGetValue<double>(PropertyFloat.ArmorPierce, out ArmorPierce, 0);

            comAttributes.TryGetValue<double>(PropertyFloat.DamageAmplify, out DamageAmplify, 0);
            comAttributes.TryGetValue<double>(PropertyFloat.DamageResistance, out DamageResistance, 0);
            comAttributes.TryGetValue<double>(PropertyFloat.InGameAmplify, out InGameAmplify, 0);
            comAttributes.TryGetValue<double>(PropertyFloat.InGameResistance, out InGameResistance, 0f);
            comAttributes.TryGetValue<double>(PropertyFloat.Tenacity, out Tenacity, 0f);
            comAttributes.TryGetValue<double>(PropertyFloat.ScaleHpBase, out ScaleHpBase, 0f);
            comAttributes.TryGetValue<double>(PropertyFloat.ScaleAtkBase, out ScaleAtkBase, 0f);
            comAttributes.TryGetValue<double>(PropertyFloat.ScaleHp, out ScaleHp, 0f);
            comAttributes.TryGetValue<double>(PropertyFloat.ScaleAtk, out ScaleAtk, 0f);
            comAttributes.TryGetValue<double>(PropertyFloat.ScaleDef, out ScaleDef, 0f);
            comAttributes.TryGetValue<double>(PropertyFloat.FinalAtk, out FinalAtk, 0f);

            Add_FromPlayerCategoryVolume(entity);

            InstantKillImmune = entity.IsImmuneToInstantKill;
            return true;
        }

        public void Add_FromPlayerCategoryVolume(LogicEntity entity)
        {
            if (entity == null)
                return;

            if (!entity.GetBattleUnitTag(out var unitTag))
            {
                BattleLogger.LogError("叠加 玩家身上记录的集体生效的分类属性 unitTag == null");
                return;
            }

            var playerInfo = entity.GetPlayerInfo();
            if (playerInfo == null)
                return;

            Add(playerInfo.VolumePlayer.Property);

            var volumeInfoBattleUnit = playerInfo.GetVolume_BattleUnit(unitTag.BattleUnitId ?? 0);
            if (volumeInfoBattleUnit != null)
                Add(volumeInfoBattleUnit.Property);

            var volumeInfoCamp = playerInfo.GetVolume_Camp(unitTag.Camp ?? 0);
            if (volumeInfoCamp != null)
                Add(volumeInfoCamp.Property);

            var volumeInfoUnitType = playerInfo.GetVolume_UnitType(unitTag.BattleUnitType ?? 0);
            if (volumeInfoUnitType != null)
                Add(volumeInfoUnitType.Property);

            var volumeInfoAttackType = playerInfo.GetVolume_AttackType(unitTag.AttackType ?? 0);
            if (volumeInfoAttackType != null)
                Add(volumeInfoAttackType.Property);
            // 元素词条按本次伤害 TElementType 在 IDamagePropertyModifier 结算时叠加，不绑 BattleUnitTag。
        }

        public void FillFromSubobjectVolume(IBattlePlayerInfo playerInfo, uint subobjectTid)
        {
            if (playerInfo is not InGamePlayerInfo inGamePlayerInfo)
            {
                BattleLogger.LogError("叠加 玩家身上记录的集体生效的分类属性 playerInfo 不是 InGamePlayerInfo");
                return;
            }

            var volumeSubobjectTid = inGamePlayerInfo.GetVolume_SubobjectTid((int)subobjectTid);
            if (volumeSubobjectTid != null)
                Add(volumeSubobjectTid.Property);
        }

        public void Add(in PropertySnapshot other)
        {
            Health += other.Health;
            Attack += other.Attack;
            Defense += other.Defense;

            Hit += other.Hit;
            Dodge += other.Dodge;

            Crit += other.Crit;
            CritResist += other.CritResist;
            CritDamage += other.CritDamage;

            InstantKill += other.InstantKill;
            InstantKillResist += other.InstantKillResist;

            ArmorPierce += other.ArmorPierce;

            DamageAmplify += other.DamageAmplify;
            DamageResistance += other.DamageResistance;
            InGameAmplify += other.InGameAmplify;
            InGameResistance += other.InGameResistance;

            Tenacity += other.Tenacity;

            ScaleHpBase += other.ScaleHpBase;
            ScaleAtkBase += other.ScaleAtkBase;
            ScaleHp += other.ScaleHp;
            ScaleAtk += other.ScaleAtk;
            ScaleDef += other.ScaleDef;

            FinalAtk += other.FinalAtk;

            VsBossAmplify += other.VsBossAmplify;

            Feature += other.Feature;
        }

        public static PropertySnapshot operator +(PropertySnapshot a, PropertySnapshot b)
        {
            PropertySnapshot result = a;
            result.Add(b);
            return result;
        }
    }
}
