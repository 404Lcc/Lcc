using UnityEngine;

namespace LccModel
{
    public class SpellSkillComponent : Component
    {
        public Combat Combat => GetParent<Combat>();

        public void Spell(SkillAbility spellSkill)
        {
            if (Combat.spellingSkillExecution != null) return;
            if (Combat.spellSkillActionAbility.TryMakeAction(out var spellAction))
            {
                spellAction.skillAbility = spellSkill;
                spellAction.SpellSkill();
            }
        }

        public void SpellWithTarget(SkillAbility spellSkill, Combat target)
        {
            if (Combat.spellingSkillExecution != null) return;
            spellSkill.Owner.TransformComponent.rotation = Quaternion.LookRotation(target.TransformComponent.position - spellSkill.Owner.TransformComponent.position);
            if (Combat.spellSkillActionAbility.TryMakeAction(out var spellAction))
            {
                spellAction.skillAbility = spellSkill;
                spellAction.inputTarget = target;
                spellAction.inputDirection = spellSkill.Owner.TransformComponent.rotation.eulerAngles.y;
                spellAction.SpellSkill();
            }
        }

        public void SpellWithPoint(SkillAbility spellSkill, Vector3 point)
        {
            if (Combat.spellingSkillExecution != null) return;
            spellSkill.Owner.TransformComponent.rotation = Quaternion.LookRotation(point - spellSkill.Owner.TransformComponent.position);
            if (Combat.spellSkillActionAbility.TryMakeAction(out var spellAction))
            {
                spellAction.skillAbility = spellSkill;
                spellAction.inputPoint = point;
                spellAction.inputDirection = spellSkill.Owner.TransformComponent.rotation.eulerAngles.y;
                spellAction.SpellSkill();
            }
        }
    }
}