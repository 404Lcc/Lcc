namespace LccModel
{
    public class AbilityEffectIntervalTriggerComponent : Component
    {
        public Effect Effect => GetParent<AbilityEffect>().effect;
        public string IntervalValueFormula => Effect.IntervalValueFormula;

        public long intervalTimer;


        public override void Awake()
        {
            base.Awake();

            long interval = ExpressionUtil.Evaluate<long>(IntervalValueFormula, GetParent<AbilityEffect>().GetParamsDict());

            intervalTimer = Timer.Instance.NewRepeatedTimer(interval, GetParent<AbilityEffect>().TryAssignEffectToOwner);
        }

        public override void OnDestroy()
        {
            base.OnDestroy();
            Timer.Instance.RemoveTimer(intervalTimer);

        }
    }
}