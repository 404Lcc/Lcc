namespace LccModel
{
    public class ExecutionEffectTimeTriggerComponent : Component, IUpdate
    {
        public float startTime;
        public float endTime;
        public string timeValueExpression;
        public GameTimer startTimer;
        public GameTimer endTimer;

        public override void Awake()
        {
            if (!string.IsNullOrEmpty(timeValueExpression))
            {
                startTime = ExpressionUtil.Evaluate<int>(timeValueExpression) / 1000f;
                startTimer = new GameTimer(startTime);
            }
            else if (startTime > 0)
            {
                startTimer = new GameTimer(startTime);
            }
            else
            {
                GetParent<ExecutionEffect>().StartTriggerEffect();
            }

            if (endTime > 0)
            {
                endTimer = new GameTimer(endTime);
            }

        }
        public void Update()
        {
            if (startTimer != null && startTimer.IsFinished == false)
            {
                startTimer.UpdateAsFinish(UnityEngine.Time.deltaTime, GetParent<ExecutionEffect>().StartTriggerEffect);
            }
            if (endTimer != null && endTimer.IsFinished == false)
            {
                endTimer.UpdateAsFinish(UnityEngine.Time.deltaTime, GetParent<ExecutionEffect>().EndEffect);
            }
        }
    }
}