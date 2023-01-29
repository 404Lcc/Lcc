namespace LccModel
{
    public class ExecutionEffectTimeTriggerComponent : Component, IUpdate
    {
        public override bool DefaultEnable => false;

        public float startTime;
        public float endTime;
        public string timeValueExpression;
        public GameTimer startTimer;
        public GameTimer endTimer;


        public void Update()
        {
            if (startTimer != null && startTimer.IsFinished == false)
            {
                startTimer.UpdateAsFinish(UnityEngine.Time.deltaTime, GetParent<ExecutionEffect>().TriggerEffect);
            }
            if (endTimer != null && endTimer.IsFinished == false)
            {
                endTimer.UpdateAsFinish(UnityEngine.Time.deltaTime, GetParent<ExecutionEffect>().EndEffect);
            }
        }

        public override void OnEnable()
        {

            if (!string.IsNullOrEmpty(timeValueExpression))
            {
                var expression = ExpressionHelper.TryEvaluate(timeValueExpression);
                startTime = (int)expression.Value / 1000f;
                startTimer = new GameTimer(startTime);
            }
            else if (startTime > 0)
            {
                startTimer = new GameTimer(startTime);
            }
            else
            {
                GetParent<ExecutionEffect>().TriggerEffect();
            }

            if (endTime > 0)
            {
                endTimer = new GameTimer(endTime);
            }
        }
    }
}