namespace LccModel
{
    /// <summary>
    /// 状态的生命周期组件
    /// </summary>
    public class StatusLifeTimeComponent : Component, IUpdate
    {
        public override bool DefaultEnable => true;
        public GameTimer LifeTimer { get; set; }


        public override void Awake()
        {
            var lifeTime = GetParent<StatusAbility>().GetDuration() / 1000f;
            LifeTimer = new GameTimer(lifeTime);
        }

        public void Update()
        {
            if (LifeTimer.IsRunning)
            {
                LifeTimer.UpdateAsFinish(UnityEngine.Time.deltaTime, OnLifeTimeFinish);
            }
        }

        private void OnLifeTimeFinish()
        {
            GetParent<StatusAbility>().EndAbility();
        }
    }
}