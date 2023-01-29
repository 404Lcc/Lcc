using System;
using UnityEngine;

namespace LccModel
{
    public class CombatEntity : Entity, IPosition
    {

        public HealthPoint currentHealth;

        public ActionControlType actionControlType; //��Ϊ����
        #region ��������

        public AttackAbility attackAbility; //�չ�����



        #endregion



        #region �ж�����

        public EffectAssignActionAbility effectAssignActionAbility;//Ч�������ж�����
        public AddStatusActionAbility addStatusActionAbility;  //ʩ��״̬�ж�����

        public SpellAttackActionAbility spellAttackActionAbility;//ʩ���չ��ж�����
        public AttackBlockActionAbility attackBlockActionAbility; //�չ�������
        public SpellSkillActionAbility spellSkillActionAbility; //ʩ�������ж�����

        public SpellItemActionAbility spellItemActionAbility;  //ʹ����Ʒ�ж�����


        public DamageActionAbility damageActionAbility;  //�˺��ж�����
        public CureActionAbility cureActionAbility;  //�����ж�����






        #endregion

        public AttackExecution spellingAttackExecution; //ִ���еĹ���ִ����
        public SkillExecution spellingSkillExecution; //ִ���еļ���ִ����



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

        #region �ӿ�

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
        public bool HasStatus(int statusId)
        {
            return GetComponent<StatusComponent>().statusDict.ContainsKey(statusId);
        }

        public StatusAbility GetStatus(int statusId)
        {
            return GetComponent<StatusComponent>().statusDict[statusId][0];
        }
        #endregion

        #region ����
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

        #region �ж����¼�
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

        #region �����¼�
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