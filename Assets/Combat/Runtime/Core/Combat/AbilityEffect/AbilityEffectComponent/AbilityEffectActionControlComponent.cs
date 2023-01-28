namespace LccModel
{
    public class AbilityEffectActionControlComponent : Component
    {
        public override bool DefaultEnable => false;
        public ActionControlEffect ActionControlEffect { get; set; }
        public ActionControlType ActionControlType { get; set; }


        public override void Awake()
        {
            ActionControlEffect = GetParent<AbilityEffect>().EffectConfig as ActionControlEffect;
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