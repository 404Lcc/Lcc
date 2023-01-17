namespace LccModel
{
    /// <summary>
    /// 行动禁制效果组件
    /// </summary>
    public class EffectActionControlComponent : Component
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
            //Parent.Parent.Parent.GetComponent<StatusComponent>().OnStatusesChanged(Entity.GetParent<StatusAbility>());
        }

        public override void OnDisable()
        {
            //Parent.Parent.Parent.GetComponent<StatusComponent>().OnStatusesChanged(Entity.GetParent<StatusAbility>());
        }
    }
}