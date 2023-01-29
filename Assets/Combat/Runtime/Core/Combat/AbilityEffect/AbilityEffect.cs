namespace LccModel
{
    /// <summary>
    /// 能力效果，如伤害、治疗、施加状态等这些和技能数值、状态相关的效果
    /// </summary>
    public partial class AbilityEffect : Entity
    {
        public bool enable;
        public Effect effectConfig;

        public Entity OwnerAbility => (Entity)Parent; //AbilityEffect是挂在能力上的
        public CombatEntity OwnerEntity => (OwnerAbility as IAbilityEntity).OwnerEntity;


        public override void Awake<P1>(P1 p1)
        {
            base.Awake(p1);

            this.effectConfig = p1 as Effect;


            if (this.effectConfig is ActionControlEffect)
            {
                AddComponent<AbilityEffectActionControlComponent>();
            }

            if (this.effectConfig is AddStatusEffect)
            {
                AddComponent<AbilityEffectAddStatusComponent>();
            }

            if (this.effectConfig is AttributeModifyEffect)
            {
                AddComponent<AbilityEffectAttributeModifyComponent>();
            }

            if (this.effectConfig is ClearAllStatusEffect)
            {
            }

            if (this.effectConfig is CureEffect)
            {
                AddComponent<AbilityEffectCureComponent>();
            }

            if (this.effectConfig is CustomEffect)
            {
                AddComponent<AbilityEffectCustomComponent>();
            }

            if (this.effectConfig is DamageEffect)
            {
                AddComponent<AbilityEffectDamageComponent>();
            }

            if (this.effectConfig is RemoveStatusEffect)
            {
            }

            if (this.effectConfig is DamageBloodSuckEffect)
            {
                AddComponent<AbilityEffectDamageBloodSuckComponent>();
            }




            // 效果修饰
            AddComponent<AbilityEffectDecoratosComponent>();

            var triggable = !(this.effectConfig is ActionControlEffect) && !(this.effectConfig is AttributeModifyEffect);
            if (triggable)
            {
                // 立即触发
                if (effectConfig.EffectTriggerType == EffectTriggerType.Instant)
                {
                    TryAssignEffectToOwner();
                }
                // 行动点触发
                var isAction = effectConfig.EffectTriggerType == EffectTriggerType.Action;
                if (isAction)
                {
                    AddComponent<AbilityEffectActionTriggerComponent>();
                }
                // 间隔触发
                var isInterval = effectConfig.EffectTriggerType == EffectTriggerType.Interval && !string.IsNullOrEmpty(effectConfig.Interval);
                if (isInterval)
                {
                    AddComponent<AbilityEffectIntervalTriggerComponent>();
                }
                // 条件触发
                var isCondition = effectConfig.EffectTriggerType == EffectTriggerType.Condition && !string.IsNullOrEmpty(effectConfig.ConditionParams);
                if (isCondition)
                {
                    AddComponent<AbilityEffectConditionTriggerComponent>();
                }
            }
        }


        public override void OnDestroy()
        {
            DisableEffect();
        }

        public void EnableEffect()
        {
            enable = true;
            foreach (var item in Components.Values)
            {
                ((Component)item).Enable = true;
            }
        }

        public void DisableEffect()
        {
            enable = false;
            foreach (var item in Components.Values)
            {
                ((Component)item).Enable = false;
            }
        }


        public void TryTriggerEffect()
        {
            this.FireEvent(nameof(TryTriggerEffect));
        }





        public void TryAssignEffectToOwner()
        {
            TryAssignEffectToTarget((OwnerAbility as IAbilityEntity).OwnerEntity);
        }


        public void TryAssignEffectToParent()
        {
            TryAssignEffectToTarget((OwnerAbility as IAbilityEntity).ParentEntity);
        }


        public void TryAssignEffectToTarget(CombatEntity targetEntity)
        {
            if (OwnerEntity.effectAssignActionAbility.TryMakeAction(out var action))
            {
                action.Target = targetEntity;
                action.sourceAbility = OwnerAbility;
                action.abilityEffect = this;
                action.ApplyEffectAssign();
            }
        }

        public void TryAssignEffectToTarget(CombatEntity targetEntity, IActionExecution actionExecution)
        {
            if (OwnerEntity.effectAssignActionAbility.TryMakeAction(out var action))
            {
                action.Target = targetEntity;
                action.sourceAbility = OwnerAbility;
                action.abilityEffect = this;
                action.actionExecution = actionExecution;
                action.ApplyEffectAssign();
            }
        }

        public void TryAssignEffectToTarget(CombatEntity targetEntity, IAbilityExecution abilityExecution)
        {
            if (OwnerEntity.effectAssignActionAbility.TryMakeAction(out var action))
            {
                action.Target = targetEntity;
                action.sourceAbility = OwnerAbility;
                action.abilityEffect = this;
                action.abilityExecution = abilityExecution;
                action.ApplyEffectAssign();
            }
        }

        public void TryAssignEffectToTarget(CombatEntity targetEntity, AbilityItem abilityItem)
        {
            if (OwnerEntity.effectAssignActionAbility.TryMakeAction(out var action))
            {
                action.Target = targetEntity;
                action.sourceAbility = OwnerAbility;
                action.abilityEffect = this;
                action.abilityItem = abilityItem;
                action.ApplyEffectAssign();
            }
        }



        public void StartAssignEffect(EffectAssignAction effectAssignAction)
        {
            this.FireEvent(nameof(StartAssignEffect), effectAssignAction);
        }
    }
}