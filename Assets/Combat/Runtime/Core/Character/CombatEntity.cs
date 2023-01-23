using System;

namespace LccModel
{
    public class CombatEntity : Entity
    {
        public HealthPoint CurrentHealth { get; private set; }

        #region 自身能力
        //普攻能力
        public AttackAbility AttackAbility { get; set; }



        #endregion



        #region 行动能力
        //效果赋给行动能力
        public EffectAssignAbility EffectAssignAbility { get; private set; }
        //施法普攻行动能力
        public AttackActionAbility SpellAttackAbility { get; private set; }

        //伤害行动能力
        public DamageActionAbility DamageAbility { get; private set; }
        //治疗行动能力
        public CureActionAbility CureAbility { get; private set; }
        #endregion

        /// 行为禁制
        public ActionControlType ActionControlType { get; set; }

        public override void Awake()
        {
            base.Awake();

            AddComponent<AttributeComponent>();
            AddComponent<ActionPointComponent>();
            AddComponent<ConditionComponent>();
            AddComponent<StatusComponent>();



            CurrentHealth = AddChildren<HealthPoint>();
            CurrentHealth.current = GetComponent<AttributeComponent>().HealthPoint;
            CurrentHealth.max = GetComponent<AttributeComponent>().HealthPointMax;
            CurrentHealth.Reset();


            AttackAbility = AttachAttack();

            EffectAssignAbility = AttachAction<EffectAssignAbility>();
            SpellAttackAbility = AttachAction<AttackActionAbility>();
            DamageAbility = AttachAction<DamageActionAbility>();
            CureAbility = AttachAction<CureActionAbility>();




        }



        public T AttachAbility<T>(object configObject) where T : Entity, IAbilityEntity
        {
            var ability = AddChildren<T>(configObject);
            ability.AddComponent<AbilityLevelComponent>();
            return ability;
        }

        public T AttachAction<T>() where T : Entity, IActionAbility
        {
            var action = AddChildren<T>();
            action.AddComponent<ActionComponent>();
            action.Enable = true;
            return action;
        }

        public AttackAbility AttachAttack()
        {
            var attack = AttachAbility<AttackAbility>(null);
            return attack;
        }

        public StatusAbility AttachStatus(object configObject)
        {
            return GetComponent<StatusComponent>().AttachStatus(configObject);
        }

        public void OnStatusRemove(StatusAbility statusAbility)
        {
            GetComponent<StatusComponent>().OnStatusRemove(statusAbility);
        }


        #region 行动点事件
        public void ListenActionPoint(ActionPointType actionPointType, Action<Entity> action)
        {
            GetComponent<ActionPointComponent>().AddListener(actionPointType, action);
        }

        public void UnListenActionPoint(ActionPointType actionPointType, Action<Entity> action)
        {
            GetComponent<ActionPointComponent>().RemoveListener(actionPointType, action);
        }

        public void TriggerActionPoint(ActionPointType actionPointType, Entity action)
        {
            GetComponent<ActionPointComponent>().TriggerActionPoint(actionPointType, action);
        }
        #endregion

        #region 条件事件
        public void ListenerCondition(ConditionType conditionType, Action action, object paramObj = null)
        {
            GetComponent<ConditionComponent>().AddListener(conditionType, action, paramObj);
        }

        public void UnListenCondition(ConditionType conditionType, Action action)
        {
            GetComponent<ConditionComponent>().RemoveListener(conditionType, action);
        }
        #endregion
    }
}