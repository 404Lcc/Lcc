namespace LccModel
{
    /// <summary>
    /// 间隔触发组件
    /// </summary>
    public class AbilityEffectIntervalTriggerComponent : Component, IUpdate
    {
        public Effect effect;
        public string intervalValueFormula;

        public GameTimer intervalTimer;


        public override void Awake()
        {
            base.Awake();

            effect = GetParent<AbilityEffect>().effectConfig;
            intervalValueFormula = effect.Interval;


            var interval = ExpressionHelper.Evaluate<int>(intervalValueFormula, GetParent<AbilityEffect>().GetParamsDict()) / 1000f;
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