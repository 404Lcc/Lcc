using System.Collections.Generic;
using UnityEngine;

namespace LccModel
{
    public class SpellSkillActionAbility : Entity, IActionAbility
    {
        public CombatEntity OwnerEntity { get => GetParent<CombatEntity>(); set { } }
        public bool Enable { get; set; }


        public bool TryMakeAction(out SpellSkillAction action)
        {
            if (Enable == false)
            {
                action = null;
            }
            else
            {
                action = OwnerEntity.AddChildren<SpellSkillAction>();
                action.ActionAbility = this;
                action.Creator = OwnerEntity;
            }
            return Enable;
        }
    }

    /// <summary>
    /// 施法行动
    /// </summary>
    public class SpellSkillAction : Entity, IActionExecution, IUpdate
    {
        public SkillAbility skillAbility;
        public SkillExecution skillExecution;
        public List<CombatEntity> inputSkillTargetList = new List<CombatEntity>();
        public CombatEntity inputTarget;
        public Vector3 inputPoint;
        public float inputDirection;

        // 行动能力
        public Entity ActionAbility { get; set; }
        // 效果赋给行动源
        public EffectAssignAction SourceAssignAction { get; set; }
        // 行动实体
        public CombatEntity Creator { get; set; }
        // 目标对象
        public CombatEntity Target { get; set; }


        public void FinishAction()
        {
            Dispose();
        }

        //前置处理
        private void PreProcess()
        {
            Creator.TriggerActionPoint(ActionPointType.PreSpell, this);
        }

        public void SpellSkill(bool actionOccupy = true)
        {
            PreProcess();
            skillExecution = skillAbility.CreateExecution() as SkillExecution;
            if (inputSkillTargetList.Count > 0)
            {
                skillExecution.inputSkillTargetList.AddRange(inputSkillTargetList);
            }
            skillExecution.actionOccupy = actionOccupy;
            skillExecution.inputTarget = inputTarget;
            skillExecution.inputPoint = inputPoint;
            skillExecution.inputDirection = inputDirection;
            skillExecution.BeginExecute();
        }

        public void Update()
        {
            if (skillExecution != null)
            {
                if (skillExecution.IsDisposed)
                {
                    PostProcess();
                    FinishAction();
                }
            }
        }

        //后置处理
        private void PostProcess()
        {
            Creator.TriggerActionPoint(ActionPointType.PostSpell, this);
        }
    }
}