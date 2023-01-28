namespace LccModel
{
    /// <summary>
    /// 时间触发组件
    /// </summary>
    public class ExecutionEffectTimeTriggerComponent : Component, IUpdate
    {
        public override bool DefaultEnable { get; set; } = false;
        public float StartTime { get; set; }
        public float EndTime { get; set; }
        public string TimeValueExpression { get; set; }
        public GameTimer StartTimer { get; set; }
        public GameTimer EndTimer { get; set; }


        public void Update()
        {
            if (StartTimer != null && StartTimer.IsFinished == false)
            {
                StartTimer.UpdateAsFinish(UnityEngine.Time.deltaTime, GetParent<ExecutionEffect>().TriggerEffect);
            }
            if (EndTimer != null && EndTimer.IsFinished == false)
            {
                EndTimer.UpdateAsFinish(UnityEngine.Time.deltaTime, GetParent<ExecutionEffect>().EndEffect);
            }
        }

        public override void OnEnable()
        {

            if (!string.IsNullOrEmpty(TimeValueExpression))
            {
                var expression = ExpressionHelper.TryEvaluate(TimeValueExpression);
                StartTime = (int)expression.Value / 1000f;
                StartTimer = new GameTimer(StartTime);
            }
            else if (StartTime > 0)
            {
                StartTimer = new GameTimer(StartTime);
            }
            else
            {
                GetParent<ExecutionEffect>().TriggerEffect();
            }

            if (EndTime > 0)
            {
                EndTimer = new GameTimer(EndTime);
            }
        }
    }
}