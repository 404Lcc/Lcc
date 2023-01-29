namespace LccModel
{
    public class AbilityEffectActionControlComponent : Component
    {
        public override bool DefaultEnable => false;
        public ActionControlEffect actionControlEffect;


        public override void Awake()
        {
            actionControlEffect = GetParent<AbilityEffect>().effectConfig as ActionControlEffect;
        }

        public override void OnEnable()
        {
            Parent.Parent.Parent.GetComponent<StatusComponent>().OnStatusesChanged(Parent.GetParent<StatusAbility>());
        }

        public override void OnDisable()
        {
            Parent.Parent.Parent.GetComponent<StatusComponent>().OnStatusesChanged(Parent.GetParent<StatusAbility>());
        }
    }
}