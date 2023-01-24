using System;
using UnityEngine;

namespace LccModel
{
    public class CombatEntity : Entity, IPosition
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
        public AttackBlockActionAbility AttackBlockAbility { get; set; }



        //伤害行动能力
        public DamageActionAbility DamageAbility { get; private set; }
        //治疗行动能力
        public CureActionAbility CureAbility { get; private set; }

        //施加状态行动能力
        public AddStatusActionAbility AddStatusAbility { get; private set; }



        #endregion

        //执行中的执行体
        public SkillExecution SpellingExecution { get; set; }



        public Vector3 Position { get; set; }
        public Quaternion Rotation { get; set; }

        /// 行为禁制
        public ActionControlType ActionControlType { get; set; }

        public override void Awake()
        {
            base.Awake();

            AddComponent<AttributeComponent>();
            AddComponent<ActionPointComponent>();
            AddComponent<ConditionComponent>();
            AddComponent<StatusComponent>();


            AddComponent<MotionComponent>();
            

            CurrentHealth = AddChildren<HealthPoint>();
            CurrentHealth.current = GetComponent<AttributeComponent>().HealthPoint;
            CurrentHealth.max = GetComponent<AttributeComponent>().HealthPointMax;
            CurrentHealth.Reset();


            AttackAbility = AttachAttack();

            EffectAssignAbility = AttachAction<EffectAssignAbility>();
            SpellAttackAbility = AttachAction<AttackActionAbility>();
            AttackBlockAbility = AttachAction<AttackBlockActionAbility>();

            DamageAbility = AttachAction<DamageActionAbility>();
            CureAbility = AttachAction<CureActionAbility>();
            AddStatusAbility = AttachAction<AddStatusActionAbility>();



        }

        #region 接口

        public void ReceiveDamage(IActionExecution combatAction)
        {
            var damageAction = combatAction as DamageAction;
            CurrentHealth.Minus(damageAction.DamageValue);
        }

        public void ReceiveCure(IActionExecution combatAction)
        {
            var cureAction = combatAction as CureAction;
            CurrentHealth.Add(cureAction.CureValue);
        }

        public bool CheckDead()
        {
            return CurrentHealth.Value <= 0;
        }
        #endregion

        #region 能力
        public T AttachAbility<T>(object configObject) where T : Entity, IAbilityEntity
        {
            var ability = AddChildren<T>(configObject);
            ability.AddComponent<AbilityLevelComponent>();
            return ability;
        }
        #endregion

        #region 行动
        public T AttachAction<T>() where T : Entity, IActionAbility
        {
            var action = AddChildren<T>();
            action.AddComponent<ActionComponent>();
            action.Enable = true;
            return action;
        }
        #endregion

        #region 普攻
        public AttackAbility AttachAttack()
        {
            var attack = AttachAbility<AttackAbility>(null);
            return attack;
        }
        #endregion

        #region 状态（buff）
        public StatusAbility AttachStatus(object configObject)
        {
            return GetComponent<StatusComponent>().AttachStatus(configObject);
        }

        public void OnStatusRemove(StatusAbility statusAbility)
        {
            GetComponent<StatusComponent>().OnStatusRemove(statusAbility);
        }
        public bool HasStatus(string statusTypeId)
        {
            return GetComponent<StatusComponent>().TypeIdStatuses.ContainsKey(statusTypeId);
        }

        public StatusAbility GetStatus(string statusTypeId)
        {
            return GetComponent<StatusComponent>().TypeIdStatuses[statusTypeId][0];
        }
        #endregion


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