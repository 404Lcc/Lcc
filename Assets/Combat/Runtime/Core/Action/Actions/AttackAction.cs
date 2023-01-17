﻿using ET;

namespace LccModel
{
    public class AttackActionAbility : Entity, IActionAbility
    {
        public CombatEntity OwnerEntity { get { return GetParent<CombatEntity>(); } set { } }
        public bool Enable { get; set; }


        public bool TryMakeAction(out AttackAction action)
        {
            if (Enable == false)
            {
                action = null;
            }
            else
            {
                action = OwnerEntity.AddChildren<AttackAction>();
                action.ActionAbility = this;
                action.Creator = OwnerEntity;
            }
            return Enable;
        }
    }

    /// <summary>
    /// 普攻行动
    /// </summary>
    public class AttackAction : Entity, IActionExecution
    {
        /// 行动能力
        public Entity ActionAbility { get; set; }
        /// 效果赋给行动源
        public EffectAssignAction SourceAssignAction { get; set; }
        /// 行动实体
        public CombatEntity Creator { get; set; }
        /// 目标对象
        public CombatEntity Target { get; set; }
        public AttackExecution AttackExecution { get; set; }


        public void FinishAction()
        {
            Dispose();
        }

        //前置处理
        private void PreProcess()
        {
            Creator.TriggerActionPoint(ActionPointType.PreGiveAttack, this);
            Target.TriggerActionPoint(ActionPointType.PreReceiveAttack, this);
        }

        public async ETTask ApplyAttackAwait()
        {
            PreProcess();

            await Timer.Instance.WaitAsync(1000);

            ApplyAttack();

            await Timer.Instance.WaitAsync(300);

            PostProcess();

            FinishAction();
        }

        public void ApplyAttack()
        {
            AttackExecution = Creator.AttackAbility.CreateExecution() as AttackExecution;
            AttackExecution.AttackAction = this;
            AttackExecution.BeginExecute();
        }

        //后置处理
        private void PostProcess()
        {
            Creator.TriggerActionPoint(ActionPointType.PostGiveAttack, this);
            Target.TriggerActionPoint(ActionPointType.PostReceiveAttack, this);
        }
    }
}