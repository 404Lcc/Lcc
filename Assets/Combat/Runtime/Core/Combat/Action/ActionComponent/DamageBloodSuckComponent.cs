namespace LccModel
{
    public class DamageBloodSuckComponent : Component
    {
        public CombatEntity OwnerEntity => Parent.GetParent<CombatEntity>();
        public override void Awake()
        {
            OwnerEntity.ListenActionPoint(ActionPointType.PostCauseDamage, OnCauseDamage);
        }
        public override void OnDestroy()
        {
            OwnerEntity.UnListenActionPoint(ActionPointType.PostCauseDamage, OnCauseDamage);
        }
        private void OnCauseDamage(Entity action)
        {
            DamageAction damageAction = (DamageAction)action;
            float value = damageAction.damageValue * 0.2f;

            if (OwnerEntity.cureActionAbility.TryMakeAction(out var cureAction))
            {
                cureAction.Creator = OwnerEntity;
                cureAction.Target = OwnerEntity;
                cureAction.cureValue = (int)value;
                cureAction.SourceAssignAction = null;
                cureAction.ApplyCure();
            }
        }
    }
}