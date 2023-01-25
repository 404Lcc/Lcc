using System;
using System.Collections.Generic;
using UnityEngine;

namespace LccModel
{
    public class CombatEntity : Entity, IPosition
    {
        public GameObject HeroObject { get; set; }
        public Transform ModelTrans { get; set; }


        public HealthPoint CurrentHealth { get; private set; }

        public ActionControlType ActionControlType { get; set; } // 行为禁制
        #region 自身能力

        public AttackAbility AttackAbility { get; set; }  //普攻能力


        public Dictionary<string, SkillAbility> NameSkills { get; set; } = new Dictionary<string, SkillAbility>();  //技能能力
        public Dictionary<int, SkillAbility> IdSkills { get; set; } = new Dictionary<int, SkillAbility>();   //技能能力

        #endregion



        #region 行动能力

        public EffectAssignActionAbility EffectAssignActionAbility { get; private set; }//效果赋给行动能力
        public AddStatusActionAbility AddStatusActionAbility { get; private set; }  //施加状态行动能力

        public SpellAttackActionAbility SpellAttackActionAbility { get; private set; }//施法普攻行动能力
        public AttackBlockActionAbility AttackBlockActionAbility { get; set; }
        public SpellSkillActionAbility SpellSkillActionAbility { get; private set; } //施法技能行动能力



        public DamageActionAbility DamageActionAbility { get; private set; }  //伤害行动能力
        public CureActionAbility CureActionAbility { get; private set; }  //治疗行动能力






        #endregion

        public AttackExecution SpellingAttackExecution { get; set; }  //执行中的攻击执行体
        public SkillExecution SpellingSkillExecution { get; set; }  //执行中的技能执行体



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


            CurrentHealth = AddChildren<HealthPoint>();
            CurrentHealth.current = GetComponent<AttributeComponent>().HealthPoint;
            CurrentHealth.max = GetComponent<AttributeComponent>().HealthPointMax;
            CurrentHealth.Reset();


            AttackAbility = AttachAttack();

            EffectAssignActionAbility = AttachAction<EffectAssignActionAbility>();
            AddStatusActionAbility = AttachAction<AddStatusActionAbility>();

            SpellAttackActionAbility = AttachAction<SpellAttackActionAbility>();
            AttackBlockActionAbility = AttachAction<AttackBlockActionAbility>();
            SpellSkillActionAbility = AttachAction<SpellSkillActionAbility>();

            DamageActionAbility = AttachAction<DamageActionAbility>();
            CureActionAbility = AttachAction<CureActionAbility>();




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
        public bool HasStatus(string statusTypeId)
        {
            return GetComponent<StatusComponent>().TypeIdStatuses.ContainsKey(statusTypeId);
        }

        public StatusAbility GetStatus(string statusTypeId)
        {
            return GetComponent<StatusComponent>().TypeIdStatuses[statusTypeId][0];
        }
        #endregion

        #region 技能
        public SkillAbility AttachSkill(object configObject)
        {
            var skill = AttachAbility<SkillAbility>(configObject);
            NameSkills.Add(skill.SkillConfig.Name, skill);
            IdSkills.Add(skill.SkillConfig.Id, skill);
            return skill;
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