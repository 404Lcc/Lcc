using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace LccHotfix
{
    // 伤害记录
    public struct DamageRecord
    {
        public DamageContext context;
        public DamageResult result;
        
        public int SkillId => (int) (context.Skill?.SkillTid ?? 0);
        public int SubobjectId => (int) (context.Subobject?.SubobjectTid ?? 0);
        public int AttackerId => (int) (context.Attacker.FighterTid);
        public double FinalDamage => result.FinalDamage;
        public bool IsCritical => result.IsCritical;
        public bool IsMiss => result.IsMiss;
        public bool IsInstantKill => result.IsInstantKill;
        
        public long RecordId;
        // public int DefenderId;
        // public int DamageType;
        // public double BaseDamage;
        // public long Timestamp;
        // public DamageModifierInfo[] Modifiers;
    }

    // 伤害记录器
    public class DamageRecorder
    {
        private readonly List<DamageRecord> _records = new List<DamageRecord>(1024);
        private readonly Dictionary<int, UnitDamageStats> _unitStats = new Dictionary<int, UnitDamageStats>();
        private readonly Dictionary<int, SkillDamageStats> _skillStats = new Dictionary<int, SkillDamageStats>();
        private readonly Dictionary<int, SubobjectDamageStats> _subobjectStats = new Dictionary<int, SubobjectDamageStats>();
        private readonly Dictionary<uint, SupplyDamageStats> _supplyStats = new Dictionary<uint, SupplyDamageStats>(16);
        private long _nextRecordId = 1;
        
        public void RecordDamage(in DamageContext context, in DamageResult result)
        {
            var record = new DamageRecord
            {
                RecordId = _nextRecordId++,
                context = context,
                result = result
                //AttackerId = context.Attacker.InstanceId,
                //SubobjectId = context.Subobject?.InstanceId ?? -1,
                //DamageType = context.DamageType,
                //BaseDamage = context.BaseDamage,
                // FinalDamage = result.FinalDamage,
                // IsCritical = result.IsCritical,
                // IsMiss = result.IsMiss,
                // IsInstantKill = result.IsInstantKill,
                //Timestamp = context.Timestamp,
                //Modifiers = result.Modifiers
            };
            
            _records.Add(record);
            UpdateStatistics(record);
            UpdateSupplyStatistics(context, result);
        }
        
        private void UpdateStatistics(DamageRecord record)
        {
            UpdateUnitStatistics(record);
            
            if (record.SkillId != -1)
                UpdateSkillStatistics(record);
                
            if (record.SubobjectId != -1)
                UpdateSubobjectStatistics(record);
        }

        private void UpdateSupplyStatistics(in DamageContext context, in DamageResult result)
        {
            var setId = context.BattleSupplySetId;
            if (setId == 0)
                return;

            if (!_supplyStats.TryGetValue(setId, out var stats))
            {
                stats = new SupplyDamageStats { SetId = setId };
                _supplyStats[setId] = stats;
            }

            stats.TotalDamage += Mathf.FloorToInt((float)result.FinalDamage) + Mathf.FloorToInt((float)result.ShieldDeducted);
            stats.HitCount++;
            _supplyStats[setId] = stats;
        }
        
        private void UpdateUnitStatistics(DamageRecord record)
        {
            if (!_unitStats.TryGetValue(record.AttackerId, out var stats))
            {
                stats = new UnitDamageStats { UnitId = record.AttackerId };
                _unitStats[record.AttackerId] = stats;
            }
            
            stats.TotalDamage += record.FinalDamage;
            stats.HitCount++;
            if (record.IsCritical) stats.CriticalCount++;
            if (record.IsMiss) stats.MissCount++;
            if (record.IsInstantKill) stats.InstantKillCount++;
            _unitStats[record.AttackerId] = stats;
        }
        
        private void UpdateSkillStatistics(DamageRecord record)
        {
            if (!_skillStats.TryGetValue(record.SkillId, out var stats))
            {
                stats = new SkillDamageStats { SkillId = record.SkillId };
                _skillStats[record.SkillId] = stats;
            }
            
            stats.TotalDamage += record.FinalDamage;
            stats.UseCount++;
            _skillStats[record.SkillId] = stats;
        }
        
        private void UpdateSubobjectStatistics(DamageRecord record)
        {
            if (!_subobjectStats.TryGetValue(record.SubobjectId, out var stats))
            {
                stats = new SubobjectDamageStats { SubobjectId = record.SubobjectId };
                _subobjectStats[record.SubobjectId] = stats;
            }
            
            stats.TotalDamage += record.FinalDamage;
            stats.HitCount++;
            _subobjectStats[record.SubobjectId] = stats;
        }
        
        // 统计查询方法
        public UnitDamageStats GetUnitDamageStats(int unitId)
        {
            return _unitStats.TryGetValue(unitId, out var stats) ? stats : default;
        }
        
        public SkillDamageStats GetSkillDamageStats(int skillId)
        {
            return _skillStats.TryGetValue(skillId, out var stats) ? stats : default;
        }
        
        public SubobjectDamageStats GetSubobjectDamageStats(int subobjectId)
        {
            return _subobjectStats.TryGetValue(subobjectId, out var stats) ? stats : default;
        }
        
        public IEnumerable<DamageRecord> GetDamageRecords()
        {
            return _records;
        }

        public List<UnitDamageStats> GetAllUnitDamageStats()
        {
            return _unitStats.Values.ToList();
        }

        public SupplyDamageStats GetSupplyDamageStats(uint setId)
        {
            return _supplyStats.TryGetValue(setId, out var stats) ? stats : default;
        }

        public void Clear()
        {
            _records.Clear();
            _unitStats.Clear();
            _skillStats.Clear();
            _subobjectStats.Clear();
            _supplyStats.Clear();
            _nextRecordId = 1;
        }
    }

    // 统计数据结构
    public struct UnitDamageStats
    {
        public int UnitId;
        public double TotalDamage;
        public int HitCount;
        public int CriticalCount;
        public int MissCount;
        public int InstantKillCount;
        
        public double AverageDamage => HitCount > 0 ? TotalDamage / HitCount : 0;
        public double CriticalRate => HitCount > 0 ? (double)CriticalCount / HitCount : 0;
    }

    public struct SkillDamageStats
    {
        public int SkillId;
        public double TotalDamage;
        public int UseCount;
        
        public double AverageDamage => UseCount > 0 ? TotalDamage / UseCount : 0;
    }

    public struct SubobjectDamageStats
    {
        public int SubobjectId;
        public double TotalDamage;
        public int HitCount;
        
        public double AverageDamage => HitCount > 0 ? TotalDamage / HitCount : 0;
    }

    public struct SupplyDamageStats
    {
        public uint SetId;
        public double TotalDamage;
        public int HitCount;
    }
}
