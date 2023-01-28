using System.Collections.Generic;
using UnityEngine;

namespace LccModel
{
    public class SpellSkillActionAbility : Entity, IActionAbility
    {
        public CombatEntity OwnerEntity { get { return GetParent<CombatEntity>(); } set { } }
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
        public SkillAbility SkillAbility { get; set; }
        public SkillExecution SkillExecution { get; set; }
        public List<CombatEntity> SkillTargets { get; set; } = new List<CombatEntity>();
        public CombatEntity InputTarget { get; set; }
        public Vector3 InputPoint { get; set; }
        public float InputDirection { get; set; }

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
            SkillExecution = SkillAbility.CreateExecution() as SkillExecution;
            //SkillExecution.Name = SkillAbility.Name;
            if (SkillTargets.Count > 0)
            {
                SkillExecution.SkillTargets.AddRange(SkillTargets);
            }
            SkillExecution.ActionOccupy = actionOccupy;
            SkillExecution.InputTarget = InputTarget;
            SkillExecution.InputPoint = InputPoint;
            SkillExecution.InputDirection = InputDirection;
            SkillExecution.BeginExecute();
        }

        public void Update()
        {
            if (SkillExecution != null)
            {
                if (SkillExecution.IsDisposed)
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