using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

namespace LccModel
{
    /// <summary>
    /// 技能施法组件
    /// </summary>
    public class SpellSkillComponent : Component
    {
        public override bool DefaultEnable => true;
        public CombatEntity CombatEntity => GetParent<CombatEntity>();

        public void SpellWithTarget(SkillAbility spellSkill, CombatEntity targetEntity)
        {
            if (CombatEntity.spellingSkillExecution != null) return;
            spellSkill.OwnerEntity.Rotation = Quaternion.LookRotation(targetEntity.Position - spellSkill.OwnerEntity.Position);
            if (CombatEntity.spellSkillActionAbility.TryMakeAction(out var spellAction))
            {
                spellAction.skillAbility = spellSkill;
                spellAction.inputTarget = targetEntity;
                spellAction.inputPoint = targetEntity.Position;
                spellAction.inputDirection = spellSkill.OwnerEntity.Rotation.eulerAngles.y;
                spellAction.SpellSkill();
            }
        }

        public void SpellWithPoint(SkillAbility spellSkill, Vector3 point)
        {
            if (CombatEntity.spellingSkillExecution != null) return;
            spellSkill.OwnerEntity.Rotation = Quaternion.LookRotation(point - spellSkill.OwnerEntity.Position);
            if (CombatEntity.spellSkillActionAbility.TryMakeAction(out var spellAction))
            {
                spellAction.skillAbility = spellSkill;
                spellAction.inputPoint = point;
                spellAction.inputDirection = spellSkill.OwnerEntity.Rotation.eulerAngles.y;
                spellAction.SpellSkill();
            }
        }
    }
}