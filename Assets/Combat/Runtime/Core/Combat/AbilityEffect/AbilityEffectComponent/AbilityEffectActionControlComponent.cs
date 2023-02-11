namespace LccModel
{
    public class AbilityEffectActionControlComponent : Component
    {
        public ActionControlEffect ActionControlEffect => (ActionControlEffect)GetParent<AbilityEffect>().effect;


        public override void Awake()
        {
            Parent.Parent.Parent.GetComponent<StatusComponent>().OnStatusesChanged(Parent.GetParent<StatusAbility>());
        }

        public override void OnDestroy()
        {
            Parent.Parent.Parent.GetComponent<StatusComponent>().OnStatusesChanged(Parent.GetParent<StatusAbility>());
        }
    }
}