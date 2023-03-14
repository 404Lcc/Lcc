using System.Collections.Generic;

namespace LccModel
{
    public class AttackAbility : Entity, IAbility
    {
        public bool Enable { get; set; }
        public Combat Owner => GetParent<Combat>();




        public override void Awake<P1>(P1 p1)
        {
            base.Awake(p1);

            List<Effect> effectList = new List<Effect>();

            DamageEffect damageEffect = new DamageEffect();
            damageEffect.Enabled = true;
            damageEffect.EffectTriggerType = EffectTriggerType.Condition;
            damageEffect.CanCrit = true;
            damageEffect.DamageType = DamageType.Physic;
            damageEffect.DamageValueFormula = "自身攻击力";
            effectList.Add(damageEffect);

            AddComponent<AbilityEffectComponent, List<Effect>>(effectList);

        }


        public void ActivateAbility()
        {
            Enable = true;
        }
        public void EndAbility()
        {
            Enable = false;
        }




        public Entity CreateExecution()
        {
            var execution = Owner.AddChildren<AttackExecution>(this);
            execution.Ability = this;
            return execution;
        }
    }
}