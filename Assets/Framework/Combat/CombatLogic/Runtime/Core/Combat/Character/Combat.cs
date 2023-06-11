using System;
using UnityEngine;

namespace LccModel
{
    public class Combat : Entity
    {

        public HealthPoint currentHealth;

        public ActionControlType actionControlType;//行为禁制




        #region 行动能力

        public EffectAssignActionAbility effectAssignActionAbility;//效果赋给行动能力
        public AddStatusActionAbility addStatusActionAbility;//施加状态行动能力


        public SpellSkillActionAbility spellSkillActionAbility;//施法技能行动能力

        public SpellItemActionAbility spellItemActionAbility;//使用物品行动能力


        public DamageActionAbility damageActionAbility;//伤害行动能力
        public CureActionAbility cureActionAbility;//治疗行动能力






        #endregion


        public SkillExecution spellingSkillExecution; //执行中的技能执行体



        public TransformComponent TransformComponent => GetComponent<TransformComponent>();
        public OrcaComponent OrcaComponent => GetComponent<OrcaComponent>();

        public AnimationComponent AnimationComponent => GetComponent<AnimationComponent>();
        public AttributeComponent AttributeComponent => GetComponent<AttributeComponent>();
        public AABB2DComponent AABB2DComponent => GetComponent<AABB2DComponent>();

        public TagComponent TagComponent => GetComponent<TagComponent>();

        public override void Awake()
        {
            base.Awake();

            EventSystem.Instance.Publish(new SyncCreateCombat(InstanceId));

            AddComponent<TransformComponent>();
            AddComponent<OrcaComponent>();

            AddComponent<AnimationComponent>();
            AddComponent<AABB2DComponent, Vector2, Vector2>(new Vector2(-1, -1), new Vector2(1, 1));

            AddComponent<AttributeComponent>();
            AddComponent<ActionPointComponent>();
            AddComponent<ConditionComponent>();

            AddComponent<MotionComponent>();

            AddComponent<StatusComponent>();
            AddComponent<SkillComponent>();
            AddComponent<ExecutionComponent>();
            AddComponent<ItemComponent>();

            AddComponent<SpellSkillComponent>();
            AddComponent<SpellItemComponent>();

            AddComponent<JoystickComponent>();

            currentHealth = AddChildren<HealthPoint>();

            effectAssignActionAbility = AttachAction<EffectAssignActionAbility>();
            addStatusActionAbility = AttachAction<AddStatusActionAbility>();


            spellSkillActionAbility = AttachAction<SpellSkillActionAbility>();

            spellItemActionAbility = AttachAction<SpellItemActionAbility>();

            damageActionAbility = AttachAction<DamageActionAbility>();
            cureActionAbility = AttachAction<CureActionAbility>();

            OrcaComponent.AddAgent2D(TransformComponent.position);


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



        #region 状态（buff）
        public StatusAbility AttachStatus(int statusId)
        {
            return GetComponent<StatusComponent>().AttachStatus(statusId);
        }
        public StatusAbility GetStatus(int statusId, int index = 0)
        {
            return GetComponent<StatusComponent>().GetStatus(statusId, index);
        }

        public void OnStatusRemove(StatusAbility statusAbility)
        {
            GetComponent<StatusComponent>().OnStatusRemove(statusAbility);
        }
        public bool HasStatus(int statusId)
        {
            return GetComponent<StatusComponent>().HasStatus(statusId);
        }

        public void OnStatusesChanged(StatusAbility statusAbility)
        {
            GetComponent<StatusComponent>().OnStatusesChanged(statusAbility);
        }
        
        #endregion

        #region 技能
        public SkillAbility AttachSkill(int skillId)
        {
            return GetComponent<SkillComponent>().AttachSkill(skillId);
        }
        public SkillAbility GetSkill(int skillId)
        {
            return GetComponent<SkillComponent>().GetSkill(skillId);
        }
        #endregion

        #region 执行体
        public ExecutionConfigObject AttachExecution(int executionId)
        {
            return GetComponent<ExecutionComponent>().AttachExecution(executionId);
        }
        public ExecutionConfigObject GetExecution(int executionId)
        {
            return GetComponent<ExecutionComponent>().GetExecution(executionId);
        }
        #endregion

        #region Item
        public ItemAbility AttachItem(int itemId)
        {
            return GetComponent<ItemComponent>().AttachItem(itemId);
        }
        public ItemAbility GetItem(int itemId)
        {
            return GetComponent<ItemComponent>().GetItem(itemId);
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