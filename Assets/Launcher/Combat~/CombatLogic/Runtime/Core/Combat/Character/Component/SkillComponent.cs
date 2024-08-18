using System.Collections.Generic;

namespace LccModel
{
    public class SkillComponent : Component
    {
        public Combat Combat => GetParent<Combat>();

        public Dictionary<int, SkillAbility> skillDict = new Dictionary<int, SkillAbility>();


        public SkillAbility AttachSkill(int skillId)
        {
            SkillConfigObject skillConfigObject = AssetManager.Instance.LoadRes<SkillConfigObject>(CombatContext.Instance.loader, $"Skill_{skillId}");
            
            if (skillConfigObject == null)
            {
                return null;
            }

            var skill = Combat.AttachAbility<SkillAbility>(skillConfigObject);
            if (!skillDict.ContainsKey(skill.skillConfigObject.Id))
            {
                skillDict.Add(skill.skillConfigObject.Id, skill);
            }
            return skill;
        }
        public SkillAbility GetSkill(int skillId)
        {
            if (skillDict.ContainsKey(skillId))
            {
                return skillDict[skillId];
            }
            return null;
        }
    }
}