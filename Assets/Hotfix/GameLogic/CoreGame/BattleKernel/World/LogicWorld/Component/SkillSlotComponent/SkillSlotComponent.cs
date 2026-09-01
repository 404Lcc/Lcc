using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace LccHotfix
{
    public class SkillSlotComponent : LogicComponent
    {
        public List<SkillSlot> skillSlots = new(5);
        private List<int> _availableSkillList = new(5); // 用于判定当前可用技能的缓存，仅减少GC使用

        public void Init(uint[] skillIds)
        {
            foreach (var skillId in skillIds)
            {
                var skillSlot = ReferencePool.Acquire<SkillSlot>();
                skillSlot.Init(skillId);
                skillSlots.Add(skillSlot);
            }
        }

        public float GetCD()
        {
            return skillSlots.Count > 0 ? skillSlots.Max(x => x.CdTimer) : 0;
        }

        public float GetCD(int index)
        {
            return index >= 0 && index < skillSlots.Count ? skillSlots[index].CdTimer : 0;
        }

        public void ResetCD()
        {
            foreach (var skillSlot in skillSlots)
            {
                skillSlot.CdTimer = skillSlot.Cfg.Cd;
            }
        }

        public void ResetCDBySkillTid(uint skillTid)
        {
            foreach (var skillSlot in skillSlots)
            {
                if (skillSlot.Tid == skillTid)
                {
                    skillSlot.CdTimer = skillSlot.Cfg.Cd;
                }
            }
        }

        /// <summary>
        /// 部分技能不允许和上一个技能衔接的太紧，让其它技能服从一个最小衔接CD
        /// </summary>
        /// <param name="excludeSkillTid">技能id</param>
        public void ResetCDAfterLast(uint excludeSkillTid)
        {
            var cdAfterLast = 0f;
            foreach (var skillSlot in skillSlots)
            {
                if (skillSlot.Tid == excludeSkillTid)
                {
                    cdAfterLast = skillSlot.Cfg.CdAfterLast;
                    break;
                }
            }
            foreach (var skillSlot in skillSlots)
            {
                if (skillSlot.Tid != excludeSkillTid)
                {
                    skillSlot.CdTimer = Mathf.Max(skillSlot.CdTimer, cdAfterLast);
                }
            }
        }

        /// <summary>
        /// 设置全部技能统一进入这么长的CD
        /// </summary>
        public void SetUnifiedCD(float unifiedCD)
        {
            foreach (var skillSlot in skillSlots)
            {
                skillSlot.CdTimer = unifiedCD;
            }
        }

        /// <summary>
        /// 设置全部技能统一进入至少这么长的CD
        /// </summary>
        public void SetUnifiedMinCD(float unifiedCD)
        {
            foreach (var skillSlot in skillSlots)
            {
                skillSlot.CdTimer = Mathf.Max(skillSlot.CdTimer, unifiedCD);
            }
        }

        public float GetRange(int index = 0)
        {
            if (skillSlots.Count > index)
            {
                return skillSlots[index].Cfg.Range;
            }

            return 0.5f;
        }

        public float GetMaxRange()
        {
            return skillSlots.Max(skillSlot => skillSlot.Cfg.Range);
        }

        /// <summary>
        /// 是否能对目标使用技能，判断射程与CD
        /// </summary>
        /// <param name="target"></param>
        /// <param name="index"></param>
        /// <returns></returns>
        public bool CanUseSkill(LogicEntity target, int index = -1)
        {
            if (index >= 0)
            {
                if (index >= skillSlots.Count)
                {
                    return false;
                }
                return skillSlots[index].Check(Owner, target);
            }
            foreach (var skillSlot in skillSlots)
            {
                if (skillSlot.Check(Owner, target))
                {
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// 随机返回一个可放的技能tid，遵循CanUseSkill
        /// </summary>
        /// <param name="target"></param>
        /// <returns></returns>
        public uint RandomSkillTid(LogicEntity target)
        {
            var totalWeight = 0;
            _availableSkillList.Clear();
            for (int index = 0; index < skillSlots.Count; index++)
            {
                if (CanUseSkill(target, index))
                {
                    totalWeight += skillSlots[index].Cfg.SkillWeight;
                    _availableSkillList.Add(index);
                }
            }
            if (_availableSkillList.Count == 0)
            {
                return 0;
            }
            // 无权重设置，等概率随机抽一个
            if (totalWeight == 0)
            {
                return skillSlots[_availableSkillList[UnityEngine.Random.Range(0, _availableSkillList.Count)]].Tid;
            }
            // 只有一个可用技能，就直接放
            if (_availableSkillList.Count == 1)
            {
                return skillSlots[_availableSkillList[0]].Tid;
            }
            // 有权重设置，按权重随机
            var randomWeight = UnityEngine.Random.Range(0, totalWeight);
            var sumWeight = 0;
            for (int index = 0; index < _availableSkillList.Count; index++)
            {
                sumWeight += skillSlots[_availableSkillList[index]].Cfg.SkillWeight;
                if (sumWeight > randomWeight)
                {
                    return skillSlots[_availableSkillList[index]].Tid;
                }
            }
            return 0;
        }

        public override void DisposeOnRemove()
        {
            foreach (var skillSlot in skillSlots)
            {
                ReferencePool.Release(skillSlot);
            }

            skillSlots.Clear();
            base.DisposeOnRemove();
        }
    }


    public partial class LogicEntity
    {
        public SkillSlotComponent comSkillSlot
        {
            get { return (SkillSlotComponent)GetComponent(LogicComponentsLookup.ComSkillSlot); }
        }

        public bool hasComSkillSlot
        {
            get { return HasComponent(LogicComponentsLookup.ComSkillSlot); }
        }

        public void AddComSkillSlot(uint[] skillIds)
        {
            var index = LogicComponentsLookup.ComSkillSlot;
            var component = (SkillSlotComponent)CreateComponent(index, typeof(SkillSlotComponent));
            component.Init(skillIds);
            AddComponent(index, component);
        }

        public void RemoveComSkillSlot()
        {
            RemoveComponent(LogicComponentsLookup.ComSkillSlot);
        }
    }

    public static partial class LogicComponentsLookup
    {
        private static ComponentTypeIndex _ComSkillSlotIndex = new(typeof(SkillSlotComponent));
        public static int ComSkillSlot => _ComSkillSlotIndex.Index;
    }
}