namespace LccModel
{
    public class AbilityEffectActionControlComponent : Component
    {
        public ActionControlEffect actionControlEffect;


        public override void Awake()
        {
            actionControlEffect = GetParent<AbilityEffect>().effectConfig as ActionControlEffect;

            Parent.Parent.Parent.GetComponent<StatusComponent>().OnStatusesChanged(Parent.GetParent<StatusAbility>());
        }

        public override void OnDestroy()
        {
            base.OnDestroy();

            Parent.Parent.Parent.GetComponent<StatusComponent>().OnStatusesChanged(Parent.GetParent<StatusAbility>());
        }
    }
}