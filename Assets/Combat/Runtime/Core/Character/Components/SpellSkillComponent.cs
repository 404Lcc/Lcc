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
        private CombatEntity CombatEntity => GetParent<CombatEntity>();
        public override bool DefaultEnable { get; set; } = true;


        public void SpellWithTarget(SkillAbility spellSkill, CombatEntity targetEntity)
        {
            if (CombatEntity.SpellingSkillExecution != null) return;

            if (CombatEntity.SpellSkillActionAbility.TryMakeAction(out var spellAction))
            {
                spellAction.SkillAbility = spellSkill;
                spellAction.InputTarget = targetEntity;
                spellAction.InputPoint = targetEntity.Position;
                spellSkill.OwnerEntity.Rotation = Quaternion.LookRotation(targetEntity.Position - spellSkill.OwnerEntity.Position);
                spellAction.InputDirection = spellSkill.OwnerEntity.Rotation.eulerAngles.y;
                spellAction.SpellSkill();
            }
        }

        public void SpellWithPoint(SkillAbility spellSkill, Vector3 point)
        {
            if (CombatEntity.SpellingSkillExecution != null) return;

            if (CombatEntity.SpellSkillActionAbility.TryMakeAction(out var spellAction))
            {
                spellAction.SkillAbility = spellSkill;
                spellAction.InputPoint = point;
                spellSkill.OwnerEntity.Rotation = Quaternion.LookRotation(point - spellSkill.OwnerEntity.Position);
                spellAction.InputDirection = spellSkill.OwnerEntity.Rotation.eulerAngles.y;
                spellAction.SpellSkill();
            }
        }
    }
}