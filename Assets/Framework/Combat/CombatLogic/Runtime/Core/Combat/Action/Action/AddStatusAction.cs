namespace LccModel
{
    public class AddStatusActionAbility : Entity, IActionAbility
    {
        public bool Enable { get; set; }
        public Combat Owner => GetParent<Combat>();



        public bool TryMakeAction(out AddStatusAction action)
        {
            if (Enable == false)
            {
                action = null;
            }
            else
            {
                action = Owner.AddChildren<AddStatusAction>();
                action.ActionAbility = this;
                action.Creator = Owner;
            }
            return Enable;
        }
    }

    public class AddStatusAction : Entity, IActionExecution
    {
        public Entity sourceAbility;

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

        }

        public void ApplyAddStatus()
        {
            PreProcess();

            AddStatusEffect addStatusEffect = (AddStatusEffect)SourceAssignAction.abilityEffect.effect;
            StatusConfigObject statusConfigObject = addStatusEffect.StatusConfigObject;
            StatusAbility status = null;

            if (!statusConfigObject.CanStack)
            {
                if (Target.HasStatus(statusConfigObject.Id))
                {
                    status = Target.GetStatus(statusConfigObject.Id);
                    var statusLifeTimer = status.GetComponent<StatusLifeTimeComponent>().lifeTimer;
                    statusLifeTimer.MaxTime = addStatusEffect.Duration / 1000f;
                    statusLifeTimer.Reset();
                    return;
                }
            }

            status = Target.AttachStatus(statusConfigObject.Id);
            status.Creator = Creator;
            status.GetComponent<AbilityLevelComponent>().level = sourceAbility.GetComponent<AbilityLevelComponent>().level;
            status.duration = (int)addStatusEffect.Duration;


            status.SetParams(addStatusEffect.ParamsDict);

            status.AddComponent<StatusLifeTimeComponent>();
            status.ActivateAbility();

            PostProcess();

            FinishAction();
        }


        private void PostProcess()
        {
            Creator.TriggerActionPoint(ActionPointType.PostGiveStatus, this);
            Target.TriggerActionPoint(ActionPointType.PostReceiveStatus, this);
        }
    }
}