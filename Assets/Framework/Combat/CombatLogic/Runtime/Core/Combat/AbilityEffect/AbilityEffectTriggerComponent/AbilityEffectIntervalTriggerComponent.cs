namespace LccModel
{
    public class AbilityEffectIntervalTriggerComponent : Component//, IUpdate
    {
        public Effect Effect => GetParent<AbilityEffect>().effect;
        public string IntervalValueFormula => Effect.IntervalValueFormula;

        public long intervalTimer;


        public override void Awake()
        {
            base.Awake();

            long interval = ExpressionUtil.Evaluate<long>(IntervalValueFormula, GetParent<AbilityEffect>().GetParamsDict());

            intervalTimer = Timer.Instance.NewRepeatedTimer(interval, GetParent<AbilityEffect>().TryAssignEffectToOwner);
            //intervalTimer = new GameTimer(interval);
        }

        public override void OnDestroy()
        {
            base.OnDestroy();
            Timer.Instance.RemoveTimer(intervalTimer);

        }
        //public void Update()
        //{
        //    if (intervalTimer != null)
        //    {
        //        intervalTimer.UpdateAsRepeat(UnityEngine.Time.deltaTime, GetParent<AbilityEffect>().TryAssignEffectToOwner);
        //    }
        //}
    }
}