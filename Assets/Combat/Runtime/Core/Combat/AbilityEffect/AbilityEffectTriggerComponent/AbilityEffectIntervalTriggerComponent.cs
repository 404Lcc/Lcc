namespace LccModel
{
    public class AbilityEffectIntervalTriggerComponent : Component, IUpdate
    {
        public Effect Effect => GetParent<AbilityEffect>().effect;
        public string IntervalValueFormula => Effect.IntervalValueFormula;

        public GameTimer intervalTimer;


        public override void Awake()
        {
            base.Awake();

            float interval = ExpressionUtil.Evaluate<int>(IntervalValueFormula, GetParent<AbilityEffect>().GetParamsDict()) / 1000f;
            intervalTimer = new GameTimer(interval);
        }

        public void Update()
        {
            if (intervalTimer != null)
            {
                intervalTimer.UpdateAsRepeat(UnityEngine.Time.deltaTime, GetParent<AbilityEffect>().TryAssignEffectToOwner);
            }
        }
    }
}