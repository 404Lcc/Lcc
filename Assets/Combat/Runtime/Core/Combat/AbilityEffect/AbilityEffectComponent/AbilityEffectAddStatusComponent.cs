namespace LccModel
{
    public class AbilityEffectAddStatusComponent : Component
    {
        public AddStatusEffect AddStatusEffect { get; set; }
        public uint Duration { get; set; }
        public string NumericValueProperty { get; set; }


        public override void Awake()
        {
            AddStatusEffect = GetParent<AbilityEffect>().EffectConfig as AddStatusEffect;
            Duration = AddStatusEffect.Duration;
            ((Entity)Parent).OnEvent(nameof(AbilityEffect.StartAssignEffect), OnAssignEffect);


        }

        public int GetNumericValue()
        {
            return 1;
        }

        private void OnAssignEffect(Entity entity)
        {

            var effectAssignAction = entity as EffectAssignAction;
            if (GetParent<AbilityEffect>().OwnerEntity.AddStatusActionAbility.TryMakeAction(out var action))
            {
                effectAssignAction.FillDatasToAction(action);
                action.SourceAbility = effectAssignAction.SourceAbility;
                action.ApplyAddStatus();
            }
        }
    }
}