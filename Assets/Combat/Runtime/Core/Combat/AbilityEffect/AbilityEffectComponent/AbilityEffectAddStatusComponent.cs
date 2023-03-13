namespace LccModel
{
    public class AbilityEffectAddStatusComponent : Component
    {
        public Combat Owner => GetParent<AbilityEffect>().Owner;

        public void OnAssignEffect(EffectAssignAction effectAssignAction)
        {
            if (Owner.addStatusActionAbility.TryMakeAction(out var action))
            {
                effectAssignAction.FillDatasToAction(action);
                action.sourceAbility = effectAssignAction.sourceAbility;
                action.ApplyAddStatus();
            }
        }
    }
}