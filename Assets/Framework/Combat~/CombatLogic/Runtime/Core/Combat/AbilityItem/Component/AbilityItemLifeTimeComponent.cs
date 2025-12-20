namespace LccModel
{
    public class AbilityItemLifeTimeComponent : Component
    {
        public long lifeTimer;

        public override void Awake<P1>(P1 p1)
        {
            base.Awake(p1);

            lifeTimer = Timer.Instance.NewOnceTimer((long)(object)p1, Destroy);
        }

        public override void OnDestroy()
        {
            base.OnDestroy();

            Timer.Instance.RemoveTimer(lifeTimer);
        }

        private void Destroy()
        {
            Parent.Dispose();
        }
    }
}