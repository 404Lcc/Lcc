namespace LccModel
{
    public class StatusLifeTimeComponent : Component, IUpdate
    {

        public GameTimer lifeTimer;

        public override void Awake()
        {
            var lifeTime = GetParent<StatusAbility>().duration / 1000f;
            lifeTimer = new GameTimer(lifeTime);
        }

        public void Update()
        {
            if (lifeTimer.IsRunning)
            {
                lifeTimer.UpdateAsFinish(UnityEngine.Time.deltaTime, LifeTimeFinish);
            }
        }

        private void LifeTimeFinish()
        {
            GetParent<StatusAbility>().EndAbility();
        }
    }
}