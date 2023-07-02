namespace LccModel
{
    public class AbilityItemLifeTimeComponent : Component//, IUpdate
    {
        public long lifeTimer;

        public override void Awake<P1>(P1 p1)
        {
            base.Awake(p1);

            lifeTimer = Timer.Instance.NewOnceTimer((long)(object)p1, Destroy);

            //lifeTimer = new GameTimer((float)(object)p1);
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
        //        lifeTimer.UpdateAsFinish(UnityEngine.Time.deltaTime, Destroy);
        //    }
        //}

        private void Destroy()
        {
            Parent.Dispose();
        }
    }
}