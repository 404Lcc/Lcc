namespace LccModel
{
    public class ExecutionEffectTimeTriggerComponent : Component
    {
        public long startTime;
        public long endTime;
        public long startTimer;
        public long endTimer;

        public override void Awake()
        {
            if (startTime > 0)
            {
                startTimer = Timer.Instance.NewOnceTimer(startTime, GetParent<ExecutionEffect>().StartTriggerEffect);
            }
            else
            {
                GetParent<ExecutionEffect>().StartTriggerEffect();
            }

            if (endTime > 0)
            {
                endTimer = Timer.Instance.NewOnceTimer(endTime, GetParent<ExecutionEffect>().EndEffect);
            }

        }
        public override void OnDestroy()
        {
            base.OnDestroy();

            Timer.Instance.RemoveTimer(startTime);
            Timer.Instance.RemoveTimer(endTimer);
        }
    }
}