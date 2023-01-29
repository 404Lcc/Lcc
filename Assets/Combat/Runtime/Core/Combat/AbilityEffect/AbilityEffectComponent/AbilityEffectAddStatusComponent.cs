namespace LccModel
{
    public class AbilityEffectAddStatusComponent : Component
    {
        public override void Awake()
        {
            ((Entity)Parent).OnEvent(nameof(AbilityEffect.StartAssignEffect), OnAssignEffect);
        }

        private void OnAssignEffect(Entity entity)
        {
            var effectAssignAction = entity as EffectAssignAction;
            if (GetParent<AbilityEffect>().OwnerEntity.addStatusActionAbility.TryMakeAction(out var action))
            {
                effectAssignAction.FillDatasToAction(action);
                action.sourceAbility = effectAssignAction.sourceAbility;
                action.ApplyAddStatus();
            }
        }
    }
}