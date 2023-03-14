using System.Collections.Generic;
using UnityEngine;

namespace LccModel
{
    public class SpellSkillActionAbility : Entity, IActionAbility
    {
        public bool Enable { get; set; }
        public Combat Owner => GetParent<Combat>();



        public bool TryMakeAction(out SpellSkillAction action)
        {
            if (Enable == false)
            {
                action = null;
            }
            else
            {
                action = Owner.AddChildren<SpellSkillAction>();
                action.ActionAbility = this;
                action.Creator = Owner;
            }
            return Enable;
        }
    }

    public class SpellSkillAction : Entity, IActionExecution, IUpdate
    {
        public SkillAbility skillAbility;
        public SkillExecution skillExecution;
        public List<Combat> inputSkillTargetList = new List<Combat>();
        public Combat inputTarget;
        public Vector3 inputPoint;
        public float inputDirection;


        public Entity ActionAbility { get; set; }
        public EffectAssignAction SourceAssignAction { get; set; }
        public Combat Creator { get; set; }
        public Combat Target { get; set; }


        public void FinishAction()
        {
            Dispose();
        }

        private void PreProcess()
        {
            Creator.TriggerActionPoint(ActionPointType.PreSpell, this);
        }

        public void SpellSkill(bool actionOccupy = true)
        {
            PreProcess();
            skillExecution = (SkillExecution)skillAbility.CreateExecution();
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

        private void PostProcess()
        {
            Creator.TriggerActionPoint(ActionPointType.PostSpell, this);
        }
    }
}