using System.Collections.Generic;

namespace LccHotfix
{
    public class ComSkills : LogicComponent
    {
        public Dictionary<int, SkillAbility> skillDict = new Dictionary<int, SkillAbility>();


        public SkillAbility AttachSkill(int skillId)
        {
            SkillAbility skillAbility = new SkillAbility(Owner, skillId);
            skillDict.Add(skillId, skillAbility);
            return skillAbility;
        }

        public void RemoveSkillById(int skillId)
        {
            if (skillDict.ContainsKey(skillId))
            {
                skillDict.Remove(skillId);
            }
        }


        public SkillAbility GetSkill(int skillId)
        {
            if (skillDict.ContainsKey(skillId))
            {
                return skillDict[skillId];
            }
            return null;
        }

        public SkillAbility GetSkillByType(SkillSpellType type, int index = 0)
        {
            var id = GetSkillIdByType(type, index);
            if (id == 0)
                return null;

            return GetSkill(id);
        }

        public bool CanSpellSkill(int skillId)
        {
            if (skillDict.TryGetValue(skillId, out var skill))
            {
                return skill.CanSpellSkill();
            }
            return false;
        }


        public void FastSpellSkill(int skillId, KVContext context = null)
        {
            if (skillDict.TryGetValue(skillId, out var skill))
            {
                skill.SpellSkill(context);
            }
            else
            {
                if (context != null)
                {
                    context.Clear();
                }
            }
        }

        /// <summary>
        /// 通过权重自动选择技能
        /// </summary>
        /// <param name="type"></param>
        /// <param name="kvContext"></param>
        public void FastSpellSkill(SkillSpellType type, KVContext context = null)
        {
            var skillWeight = GetSkillWeightByType(type);
            int i = GetWeightIndex(skillWeight);
            FastSpellSkill(type, i, context);
        }
        private List<int> GetSkillWeightByType(SkillSpellType type)
        {
            List<int> list = new List<int>();
            foreach (var item in skillDict.Values)
            {
                if (item.skillData.SpellType == type)
                {
                    list.Add(item.skillData.Weight);
                }
            }
            return list;
        }
        private int GetWeightIndex(List<int> weight)
        {
            int weightAll = 0;
            int addWeight = 0;
            int randNum = 0;
            foreach (int item in weight)
            {
                if (item < 0)
                {
                    Log.Error("随机出错");
                    return 0;
                }
                weightAll += item;
            }
            randNum = UnityEngine.Random.Range(1, weightAll + 1);
            for (int i = 0; i < weight.Count; i++)
            {
                addWeight += weight[i];
                if (randNum <= addWeight)
                {
                    return i;
                }
            }
            return 0;
        }
        public void FastSpellSkill(SkillSpellType type, int index, KVContext context = null)
        {
            var skill = GetSkillIdByType(type, index);
            FastSpellSkill(skill, context);
        }

        private int GetSkillIdByType(SkillSpellType type, int index = 0)
        {
            int i = 0;
            var skillId = 0;
            foreach (var item in skillDict.Values)
            {
                if (item.skillData.SpellType == type)
                {
                    if (i == index)
                    {
                        return item.skillId;
                    }
                    i++;
                }
            }
            return skillId;
        }
    }


    public partial class LogicEntity
    {

        public ComSkills comSkills { get { return (ComSkills)GetComponent(LogicComponentsLookup.ComSkills); } }
        public bool hasComSkills { get { return HasComponent(LogicComponentsLookup.ComSkills); } }

        public void AddComSkills(Dictionary<int, SkillAbility> newSkillDict)
        {
            var index = LogicComponentsLookup.ComSkills;
            var component = (ComSkills)CreateComponent(index, typeof(ComSkills));
            component.skillDict = newSkillDict;
            AddComponent(index, component);
        }

        public void ReplaceComSkills(Dictionary<int, SkillAbility> newSkillDict)
        {
            var index = LogicComponentsLookup.ComSkills;
            var component = (ComSkills)CreateComponent(index, typeof(ComSkills));
            component.skillDict = newSkillDict;
            ReplaceComponent(index, component);
        }

        public void RemoveComSkills()
        {
            RemoveComponent(LogicComponentsLookup.ComSkills);
        }
    }
    public sealed partial class LogicMatcher
    {

        static Entitas.IMatcher<LogicEntity> _matcherComSkills;

        public static Entitas.IMatcher<LogicEntity> ComSkills
        {
            get
            {
                if (_matcherComSkills == null)
                {
                    var matcher = (Entitas.Matcher<LogicEntity>)Entitas.Matcher<LogicEntity>.AllOf(LogicComponentsLookup.ComSkills);
                    matcher.ComponentNames = LogicComponentsLookup.componentNames;
                    _matcherComSkills = matcher;
                }

                return _matcherComSkills;
            }
        }
    }
    public static partial class LogicComponentsLookup
    {
        public static int ComSkills;
    }
}