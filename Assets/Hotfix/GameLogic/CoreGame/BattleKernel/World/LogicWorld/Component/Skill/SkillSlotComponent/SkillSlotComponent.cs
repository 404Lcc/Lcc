using System.Collections.Generic;
using System.Linq;
using HotUpdate.Framework;

namespace LccHotfix
{
    public class SkillSlotComponent : LogicComponent
    {
        public List<SkillSlot> skillSlots = new();

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

        public float GetRangeByIndex(int index = 0)
        {
            if (skillSlots.Count > index)
            {
                return skillSlots[index].Cfg.Range;
            }

            return 0.5f;
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