namespace LccModel
{
    public class AbilityEffectAddStatusComponent : Component
    {
        public Combat OwnerEntity => GetParent<AbilityEffect>().OwnerEntity;

        public void OnAssignEffect(EffectAssignAction effectAssignAction)
        {
            if (OwnerEntity.addStatusActionAbility.TryMakeAction(out var action))
            {
                effectAssignAction.FillDatasToAction(action);
                action.sourceAbility = effectAssignAction.sourceAbility;
                action.ApplyAddStatus();
            }
        }
    }
}