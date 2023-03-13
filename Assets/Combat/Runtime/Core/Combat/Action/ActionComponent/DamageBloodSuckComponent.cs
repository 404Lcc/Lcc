namespace LccModel
{
    public class DamageBloodSuckComponent : Component
    {
        public Combat Owner => Parent.GetParent<Combat>();
        public override void Awake()
        {
            Owner.ListenActionPoint(ActionPointType.PostCauseDamage, OnCauseDamage);
        }
        public override void OnDestroy()
        {
            Owner.UnListenActionPoint(ActionPointType.PostCauseDamage, OnCauseDamage);
        }
        private void OnCauseDamage(Entity action)
        {
            DamageAction damageAction = (DamageAction)action;
            float value = damageAction.damageValue * 0.2f;

            if (Owner.cureActionAbility.TryMakeAction(out var cureAction))
            {
                cureAction.Creator = Owner;
                cureAction.Target = Owner;
                cureAction.cureValue = (int)value;
                cureAction.SourceAssignAction = null;
                cureAction.ApplyCure();
            }
        }
    }
}