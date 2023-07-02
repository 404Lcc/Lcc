namespace LccModel
{
    public class ExecutionEffectTimeTriggerComponent : Component//, IUpdate
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
        //public void Update()
        //{
        //    if (startTimer != null && startTimer.IsFinished == false)
        //    {
        //        startTimer.UpdateAsFinish(UnityEngine.Time.deltaTime, GetParent<ExecutionEffect>().StartTriggerEffect);
        //    }
        //    if (endTimer != null && endTimer.IsFinished == false)
        //    {
        //        endTimer.UpdateAsFinish(UnityEngine.Time.deltaTime, GetParent<ExecutionEffect>().EndEffect);
        //    }
        //}
    }
}