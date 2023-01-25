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

        public ActionControlType ActionControlType { get; set; } // ��Ϊ����
        #region ��������

        public AttackAbility AttackAbility { get; set; }  //�չ�����


        public Dictionary<string, SkillAbility> NameSkills { get; set; } = new Dictionary<string, SkillAbility>();  //��������
        public Dictionary<int, SkillAbility> IdSkills { get; set; } = new Dictionary<int, SkillAbility>();   //��������

        #endregion



        #region �ж�����

        public EffectAssignActionAbility EffectAssignActionAbility { get; private set; }//Ч�������ж�����
        public AddStatusActionAbility AddStatusActionAbility { get; private set; }  //ʩ��״̬�ж�����

        public SpellAttackActionAbility SpellAttackActionAbility { get; private set; }//ʩ���չ��ж�����
        public AttackBlockActionAbility AttackBlockActionAbility { get; set; }
        public SpellSkillActionAbility SpellSkillActionAbility { get; private set; } //ʩ�������ж�����



        public DamageActionAbility DamageActionAbility { get; private set; }  //�˺��ж�����
        public CureActionAbility CureActionAbility { get; private set; }  //�����ж�����






        #endregion

        public AttackExecution SpellingAttackExecution { get; set; }  //ִ���еĹ���ִ����
        public SkillExecution SpellingSkillExecution { get; set; }  //ִ���еļ���ִ����



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

        #region �ӿ�

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

        #region ����
        public T AttachAbility<T>(object configObject) where T : Entity, IAbilityEntity
        {
            var ability = AddChildren<T, object>(configObject);
            ability.AddComponent<AbilityLevelComponent>();
            return ability;
        }
        #endregion

        #region �ж�
        public T AttachAction<T>() where T : Entity, IActionAbility
        {
            var action = AddChildren<T>();
            action.AddComponent<ActionComponent, Type>(typeof(T));
            action.Enable = true;
            return action;
        }
        #endregion

        #region �չ�
        public AttackAbility AttachAttack()
        {
            var attack = AttachAbility<AttackAbility>(null);
            return attack;
        }
        #endregion

        #region ״̬��buff��
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

        #region ����
        public SkillAbility AttachSkill(object configObject)
        {
            var skill = AttachAbility<SkillAbility>(configObject);
            NameSkills.Add(skill.SkillConfig.Name, skill);
            IdSkills.Add(skill.SkillConfig.Id, skill);
            return skill;
        }
        #endregion

        #region �ж����¼�
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

        #region �����¼�
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