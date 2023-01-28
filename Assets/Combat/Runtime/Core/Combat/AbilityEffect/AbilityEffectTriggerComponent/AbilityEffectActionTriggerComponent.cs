namespace LccModel
{
    /// <summary>
    /// 行动点触发组件
    /// </summary>
    public class AbilityEffectActionTriggerComponent : Component
    {
        public override bool DefaultEnable { get; set; } = false;


        public override void OnEnable()
        {
            var actionPointType = GetParent<AbilityEffect>().EffectConfig.ActionPointType;
            GetParent<AbilityEffect>().GetParent<StatusAbility>().OwnerEntity.ListenActionPoint(actionPointType, OnActionPointTrigger);
        }

        private void OnActionPointTrigger(Entity combatAction)
        {
            GetParent<AbilityEffect>().TryTriggerEffect();
        }
    }
}