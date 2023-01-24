namespace LccModel
{
    /// <summary>
    /// 生命周期组件
    /// </summary>
    public class LifeTimeComponent : Component, IUpdate
    {
        public override bool DefaultEnable { get; set; } = true;
        public GameTimer LifeTimer { get; set; }

        public override void Awake<P1>(P1 p1)
        {
            base.Awake(p1);

            LifeTimer = new GameTimer((float)(object)p1);
        }

        public void Update()
        {
            if (LifeTimer.IsRunning)
            {
                LifeTimer.UpdateAsFinish(UnityEngine.Time.deltaTime, DestroyEntity);
            }
        }

        private void DestroyEntity()
        {
            Parent.Dispose();
        }
    }
}