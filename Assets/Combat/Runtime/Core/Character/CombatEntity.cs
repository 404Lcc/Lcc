using System;
using UnityEngine;

namespace LccModel
{
    public class CombatEntity : Entity, IPosition
    {
        public HealthPoint CurrentHealth { get; private set; }

        #region ��������
        //�չ�����
        public AttackAbility AttackAbility { get; set; }



        #endregion



        #region �ж�����
        //Ч�������ж�����
        public EffectAssignAbility EffectAssignAbility { get; private set; }
        //ʩ���չ��ж�����
        public AttackActionAbility SpellAttackAbility { get; private set; }
        public AttackBlockActionAbility AttackBlockAbility { get; set; }



        //�˺��ж�����
        public DamageActionAbility DamageAbility { get; private set; }
        //�����ж�����
        public CureActionAbility CureAbility { get; private set; }

        //ʩ��״̬�ж�����
        public AddStatusActionAbility AddStatusAbility { get; private set; }



        #endregion

        //ִ���е�ִ����
        public SkillExecution SpellingExecution { get; set; }



        public Vector3 Position { get; set; }
        public Quaternion Rotation { get; set; }

        /// ��Ϊ����
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
            var ability = AddChildren<T>(configObject);
            ability.AddComponent<AbilityLevelComponent>();
            return ability;
        }
        #endregion

        #region �ж�
        public T AttachAction<T>() where T : Entity, IActionAbility
        {
            var action = AddChildren<T>();
            action.AddComponent<ActionComponent>();
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