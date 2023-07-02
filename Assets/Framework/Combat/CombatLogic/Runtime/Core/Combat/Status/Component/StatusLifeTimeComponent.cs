namespace LccModel
{
    public class StatusLifeTimeComponent : Component//, IUpdate
    {

        public long lifeTimer;

        public override void Awake()
        {
            long lifeTime = GetParent<StatusAbility>().duration;

            lifeTimer = Timer.Instance.NewOnceTimer(lifeTime, GetParent<StatusAbility>().EndAbility);
            //lifeTimer = new GameTimer(lifeTime);
        }

        public override void OnDestroy()
        {
            base.OnDestroy();

            Timer.Instance.RemoveTimer(lifeTimer);
        }
        //public void Update()
        //{
        //    if (lifeTimer.IsRunning)
        //    {
        //        lifeTimer.UpdateAsFinish(UnityEngine.Time.deltaTime, LifeTimeFinish);
        //    }
        //}

    }
}