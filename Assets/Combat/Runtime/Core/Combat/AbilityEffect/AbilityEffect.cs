namespace LccModel
{
    public class ApplyEffectEvent
    {
        public AbilityEffect AbilityEffect;
    }
    public enum EffectSourceType
    {
        Ability,
        Execution,
    }

    /// <summary>
    /// 能力效果，如伤害、治疗、施加状态等这些和技能数值、状态相关的效果
    /// </summary>
    public partial class AbilityEffect : Entity
    {
        public bool Enable { get; set; }
        public Entity OwnerAbility => (Entity)Parent; //AbilityEffect是挂在能力上的
        public CombatEntity OwnerEntity => (OwnerAbility as IAbilityEntity).OwnerEntity;
        public Effect EffectConfig { get; set; }
        public EffectSourceType EffectSourceType { get; set; }

        public override void Awake<P1>(P1 p1)
        {
            base.Awake(p1);

            this.EffectConfig = p1 as Effect;

            // 行动禁制
            if (this.EffectConfig is ActionControlEffect)
            {
                AddComponent<AbilityEffectActionControlComponent>();
            }
            // 施加状态效果
            if (this.EffectConfig is AddStatusEffect)
            {
                AddComponent<AbilityEffectAddStatusComponent>();
            }
            // 属性修饰效果
            if (this.EffectConfig is AttributeModifyEffect)
            {
                AddComponent<AbilityEffectAttributeModifyComponent>();
            }
            // 清除所有状态效果
            if (this.EffectConfig is ClearAllStatusEffect)
            {
            }
            // 治疗效果
            if (this.EffectConfig is CureEffect)
            {
                AddComponent<AbilityEffectCureComponent>();
            }
            // 自定义效果
            if (this.EffectConfig is CustomEffect)
            {
                AddComponent<AbilityEffectCustomComponent>();
            }
            // 伤害效果
            if (this.EffectConfig is DamageEffect)
            {
                AddComponent<AbilityEffectDamageComponent>();
            }
            // 清除状态效果
            if (this.EffectConfig is RemoveStatusEffect)
            {
            }
            // 吸血效果
            if (this.EffectConfig is DamageBloodSuckEffect)
            {
                AddComponent<AbilityEffectDamageBloodSuckComponent>();
            }




            // 效果修饰
            AddComponent<AbilityEffectDecoratosComponent>();

            var triggable = !(this.EffectConfig is ActionControlEffect) && !(this.EffectConfig is AttributeModifyEffect);
            if (triggable)
            {
                // 立即触发
                if (EffectConfig.EffectTriggerType == EffectTriggerType.Instant)
                {
                    TryAssignEffectToParent();
                }
                // 行动点触发
                var isAction = EffectConfig.EffectTriggerType == EffectTriggerType.Action;
                if (isAction)
                {
                    AddComponent<AbilityEffectActionTriggerComponent>();
                }
                // 间隔触发
                var isInterval = EffectConfig.EffectTriggerType == EffectTriggerType.Interval && !string.IsNullOrEmpty(EffectConfig.Interval);
                if (isInterval)
                {
                    AddComponent<AbilityEffectIntervalTriggerComponent>();
                }
                // 条件触发
                var isCondition = EffectConfig.EffectTriggerType == EffectTriggerType.Condition && !string.IsNullOrEmpty(EffectConfig.ConditionParam);
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
            Enable = true;
            foreach (var item in Components.Values)
            {
                ((Component)item).Enable = true;
            }
        }

        public void DisableEffect()
        {
            Enable = false;
            foreach (var item in Components.Values)
            {
                ((Component)item).Enable = false;
            }
        }

        /// <summary>
        /// 尝试触发效果
        /// </summary>
        public void TryTriggerEffect()
        {
            this.FireEvent(nameof(TryTriggerEffect));
        }

        /// <summary>
        /// 尝试触发效果
        /// </summary>
        /// <param name="action"></param>
        public void TryTriggerEffectWithAction(IActionExecution action)
        {
            TryTriggerEffectToAction(action);
        }

        /// <summary>
        /// 尝试将效果赋给施术者
        /// </summary>
        public void TryAssignEffectToOwner()
        {
            TryAssignEffectTo((OwnerAbility as IAbilityEntity).OwnerEntity);
        }

        //尝试将效果赋给父对象
        public void TryAssignEffectToParent()
        {
            TryAssignEffectTo((OwnerAbility as IAbilityEntity).ParentEntity);
        }

        /// <summary>
        /// 尝试将效果赋给目标实体
        /// </summary>
        /// <param name="targetEntity"></param>
        public void TryAssignEffectTo(CombatEntity targetEntity)
        {
            if (OwnerEntity.EffectAssignActionAbility.TryMakeAction(out var action))
            {
                action.Target = targetEntity;
                action.SourceAbility = OwnerAbility;
                action.AbilityEffect = this;
                action.ApplyEffectAssign();
            }
        }

        /// <summary>
        /// 尝试将效果赋给目标实体
        /// </summary>
        /// <param name="targetEntity"></param>
        /// <param name="abilityItem"></param>
        public void TryAssignEffectToTargetWithAbilityItem(CombatEntity targetEntity, AbilityItem abilityItem)
        {
            if (OwnerEntity.EffectAssignActionAbility.TryMakeAction(out var action))
            {
                action.Target = targetEntity;
                action.SourceAbility = OwnerAbility;
                action.AbilityEffect = this;
                action.AbilityItem = abilityItem;
                action.ApplyEffectAssign();
            }
        }

        /// <summary>
        /// 尝试将效果赋给目标行动
        /// </summary>
        /// <param name="action"></param>
        public void TryTriggerEffectToAction(IActionExecution action)
        {
            if (OwnerEntity.EffectAssignActionAbility.TryMakeAction(out var assignAction))
            {
                assignAction.SourceAbility = OwnerAbility;
                assignAction.AbilityEffect = this;
                assignAction.TargetAction = action;
                assignAction.ApplyEffectAssign();
            }
        }

        /// <summary>
        /// 开始赋给效果
        /// </summary>
        /// <param name="effectAssignAction"></param>
        public void StartAssignEffect(EffectAssignAction effectAssignAction)
        {
            this.FireEvent(nameof(StartAssignEffect), effectAssignAction);
        }
    }
}