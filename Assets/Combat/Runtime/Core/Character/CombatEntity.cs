using System;

namespace LccModel
{
    public class CombatEntity : Entity
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

        //�˺��ж�����
        public DamageActionAbility DamageAbility { get; private set; }
        //�����ж�����
        public CureActionAbility CureAbility { get; private set; }
        #endregion

        /// ��Ϊ����
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