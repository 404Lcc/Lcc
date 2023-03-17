using System;
using UnityEngine;

namespace LccModel
{
    public class Combat : Entity
    {

        public HealthPoint currentHealth;

        public ActionControlType actionControlType;//行为禁制
        #region 自身能力
        public AttackAbility attackAbility;//普攻能力



        #endregion



        #region 行动能力

        public EffectAssignActionAbility effectAssignActionAbility;//效果赋给行动能力
        public AddStatusActionAbility addStatusActionAbility;//施加状态行动能力

        public SpellAttackActionAbility spellAttackActionAbility;//施法普攻行动能力
        public AttackBlockActionAbility attackBlockActionAbility;//普攻格挡能力
        public SpellSkillActionAbility spellSkillActionAbility;//施法技能行动能力

        public SpellItemActionAbility spellItemActionAbility;//使用物品行动能力


        public DamageActionAbility damageActionAbility;//伤害行动能力
        public CureActionAbility cureActionAbility;//治疗行动能力






        #endregion

        public AttackExecution spellingAttackExecution; //执行中的攻击执行体
        public SkillExecution spellingSkillExecution; //执行中的技能执行体



        public TransformComponent TransformComponent => GetComponent<TransformComponent>();
        public AnimationComponent AnimationComponent => GetComponent<AnimationComponent>();
        public AttributeComponent AttributeComponent => GetComponent<AttributeComponent>();
        public AABB2DComponent AABB2DComponent => GetComponent<AABB2DComponent>();

        public TagComponent TagComponent => GetComponent<TagComponent>();

        public override void Awake()
        {
            base.Awake();

            EventSystem.Instance.Publish(new SyncCreateCombat(InstanceId));

            AddComponent<TransformComponent>();
            AddComponent<AnimationComponent>();
            AddComponent<AABB2DComponent, Vector2, Vector2>(new Vector2(-1, -1), new Vector2(1, 1));

            AddComponent<AttributeComponent>();
            AddComponent<ActionPointComponent>();
            AddComponent<ConditionComponent>();

            AddComponent<MotionComponent>();

            AddComponent<StatusComponent>();

            AddComponent<SpellAttackComponent>();
            AddComponent<SpellSkillComponent>();
            AddComponent<SpellItemComponent>();

            AddComponent<JoystickComponent>();

            currentHealth = AddChildren<HealthPoint>();


            attackAbility = AttachAttack();

            effectAssignActionAbility = AttachAction<EffectAssignActionAbility>();
            addStatusActionAbility = AttachAction<AddStatusActionAbility>();

            spellAttackActionAbility = AttachAction<SpellAttackActionAbility>();
            attackBlockActionAbility = AttachAction<AttackBlockActionAbility>();
            spellSkillActionAbility = AttachAction<SpellSkillActionAbility>();

            spellItemActionAbility = AttachAction<SpellItemActionAbility>();

            damageActionAbility = AttachAction<DamageActionAbility>();
            cureActionAbility = AttachAction<CureActionAbility>();


            ListenActionPoint(ActionPointType.PostReceiveDamage, (e) =>
            {
                var damageAction = e as DamageAction;
                EventSystem.Instance.Publish(new SyncDamage(InstanceId, damageAction.damageValue));
            });

            ListenActionPoint(ActionPointType.PostReceiveCure, (e) =>
            {
                var damageAction = e as CureAction;
                EventSystem.Instance.Publish(new SyncCure(InstanceId, damageAction.cureValue));
            });

        }
        public void Dead()
        {
            EventSystem.Instance.Publish(new SyncDeleteCombat(InstanceId));
            CombatContext.Instance.RemoveCombat(InstanceId);
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
        public T AttachAbility<T>(object configObject) where T : Entity, IAbility
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