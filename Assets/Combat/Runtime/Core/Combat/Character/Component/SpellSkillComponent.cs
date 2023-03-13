using UnityEngine;

namespace LccModel
{
    public class SpellSkillComponent : Component
    {
        public Combat CombatEntity => GetParent<Combat>();

        public void SpellWithTarget(SkillAbility spellSkill, Combat targetEntity)
        {
            if (CombatEntity.spellingSkillExecution != null) return;
            spellSkill.OwnerEntity.TransformComponent.rotation = Quaternion.LookRotation(targetEntity.TransformComponent.position - spellSkill.OwnerEntity.TransformComponent.position);
            if (CombatEntity.spellSkillActionAbility.TryMakeAction(out var spellAction))
            {
                spellAction.skillAbility = spellSkill;
                spellAction.inputTarget = targetEntity;
                spellAction.inputPoint = targetEntity.TransformComponent.position;
                spellAction.inputDirection = spellSkill.OwnerEntity.TransformComponent.rotation.eulerAngles.y;
                spellAction.SpellSkill();
            }
        }

        public void SpellWithPoint(SkillAbility spellSkill, Vector3 point)
        {
            if (CombatEntity.spellingSkillExecution != null) return;
            spellSkill.OwnerEntity.TransformComponent.rotation = Quaternion.LookRotation(point - spellSkill.OwnerEntity.TransformComponent.position);
            if (CombatEntity.spellSkillActionAbility.TryMakeAction(out var spellAction))
            {
                spellAction.skillAbility = spellSkill;
                spellAction.inputPoint = point;
                spellAction.inputDirection = spellSkill.OwnerEntity.TransformComponent.rotation.eulerAngles.y;
                spellAction.SpellSkill();
            }
        }
    }
}