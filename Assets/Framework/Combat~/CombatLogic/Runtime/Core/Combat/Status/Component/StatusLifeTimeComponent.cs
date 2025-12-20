namespace LccModel
{
    public class StatusLifeTimeComponent : Component
    {

        public long lifeTimer;

        public override void Awake()
        {
            long lifeTime = GetParent<StatusAbility>().duration;

            lifeTimer = Timer.Instance.NewOnceTimer(lifeTime, GetParent<StatusAbility>().EndAbility);
        }

        public override void OnDestroy()
        {
            base.OnDestroy();

            Timer.Instance.RemoveTimer(lifeTimer);
        }

    }
}