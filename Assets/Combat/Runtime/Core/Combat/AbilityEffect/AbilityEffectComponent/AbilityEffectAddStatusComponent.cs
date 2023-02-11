namespace LccModel
{
    public class AbilityEffectAddStatusComponent : Component
    {
        public CombatEntity OwnerEntity => GetParent<AbilityEffect>().OwnerEntity;
        public override void Awake()
        {
            ((Entity)Parent).OnEvent(nameof(AbilityEffect.StartAssignEffect), OnAssignEffect);
        }

        private void OnAssignEffect(Entity entity)
        {
            EffectAssignAction effectAssignAction = (EffectAssignAction)entity;
            if (OwnerEntity.addStatusActionAbility.TryMakeAction(out var action))
            {
                effectAssignAction.FillDatasToAction(action);
                action.sourceAbility = effectAssignAction.sourceAbility;
                action.ApplyAddStatus();
            }
        }
    }
}