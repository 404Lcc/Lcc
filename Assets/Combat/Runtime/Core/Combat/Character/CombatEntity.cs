using System;
using UnityEngine;

namespace LccModel
{
    public class CombatEntity : Entity, IPosition
    {

        public HealthPoint currentHealth;

        public ActionControlType actionControlType; //行为禁制
        #region 自身能力

        public AttackAbility attackAbility; //普攻能力



        #endregion



        #region 行动能力

        public EffectAssignActionAbility effectAssignActionAbility;//效果赋给行动能力
        public AddStatusActionAbility addStatusActionAbility;  //施加状态行动能力

        public SpellAttackActionAbility spellAttackActionAbility;//施法普攻行动能力
        public AttackBlockActionAbility attackBlockActionAbility; //普攻格挡能力
        public SpellSkillActionAbility spellSkillActionAbility; //施法技能行动能力

        public SpellItemActionAbility spellItemActionAbility;  //使用物品行动能力


        public DamageActionAbility damageActionAbility;  //伤害行动能力
        public CureActionAbility cureActionAbility;  //治疗行动能力






        #endregion

        public AttackExecution spellingAttackExecution; //执行中的攻击执行体
        public SkillExecution spellingSkillExecution; //执行中的技能执行体



        public Vector3 Position { get; set; }
        public Quaternion Rotation { get; set; }






        public override void Awake()
        {
            base.Awake();

            AddComponent<AttributeComponent>();
            AddComponent<ActionPointComponent>();
            AddComponent<ConditionComponent>();

            AddComponent<MotionComponent>();

            AddComponent<StatusComponent>();

            AddComponent<SpellAttackComponent>();
            AddComponent<SpellSkillComponent>();
            AddComponent<SpellItemComponent>();


            currentHealth = AddChildren<HealthPoint>();
            currentHealth.current = GetComponent<AttributeComponent>().HealthPoint;
            currentHealth.max = GetComponent<AttributeComponent>().HealthPointMax;
            currentHealth.Reset();


            attackAbility = AttachAttack();

            effectAssignActionAbility = AttachAction<EffectAssignActionAbility>();
            addStatusActionAbility = AttachAction<AddStatusActionAbility>();

            spellAttackActionAbility = AttachAction<SpellAttackActionAbility>();
            attackBlockActionAbility = AttachAction<AttackBlockActionAbility>();
            spellSkillActionAbility = AttachAction<SpellSkillActionAbility>();

            spellItemActionAbility = AttachAction<SpellItemActionAbility>();

            damageActionAbility = AttachAction<DamageActionAbility>();
            cureActionAbility = AttachAction<CureActionAbility>();




        }

        #region 接口

        public void ReceiveDamage(IActionExecution actionExecution)
        {
            var damageAction = actionExecution as DamageAction;
            currentHealth.Minus(damageAction.damageValue);
        }

        public void ReceiveCure(IActionExecution actionExecution)
        {
            var cureAction = actionExecution as CureAction;
            currentHealth.Add(cureAction.cureValue);
        }

        public bool CheckDead()
        {
            return currentHealth.Value <= 0;
        }
        #endregion

        #region 能力
        public T AttachAbility<T>(object configObject) where T : Entity, IAbilityEntity
        {
            var ability = AddChildren<T, object>(configObject);
            ability.AddComponent<AbilityLevelComponent>();
            return ability;
        }
        #endregion

        #region 行动
        public T AttachAction<T>() where T : Entity, IActionAbility
        {
            var action = AddChildren<T>();
            action.AddComponent<ActionComponent, Type>(typeof(T));
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
        public bool HasStatus(int statusId)
        {
            return GetComponent<StatusComponent>().statusDict.ContainsKey(statusId);
        }

        public StatusAbility GetStatus(int statusId)
        {
            return GetComponent<StatusComponent>().statusDict[statusId][0];
        }
        #endregion

        #region 技能
        public SkillAbility AttachSkill(object configObject)
        {
            var skill = AttachAbility<SkillAbility>(configObject);
            return skill;
        }
        #endregion

        #region Item
        public ItemAbility AttachItem(object configObject)
        {
            var item = AttachAbility<ItemAbility>(configObject);
            return item;
        }
        #endregion

        #region 行动点事件
        public void ListenActionPoint(ActionPointType type, Action<Entity> action)
        {
            GetComponent<ActionPointComponent>().AddListener(type, action);
        }

        public void UnListenActionPoint(ActionPointType type, Action<Entity> action)
        {
            GetComponent<ActionPointComponent>().RemoveListener(type, action);
        }

        public void TriggerActionPoint(ActionPointType type, Entity action)
        {
            GetComponent<ActionPointComponent>().TriggerActionPoint(type, action);
        }
        #endregion

        #region 条件事件
        public void ListenerCondition(ConditionType type, Action action, object obj = null)
        {
            GetComponent<ConditionComponent>().AddListener(type, action, obj);
        }

        public void UnListenCondition(ConditionType type, Action action)
        {
            GetComponent<ConditionComponent>().RemoveListener(type, action);
        }
        #endregion
    }
}