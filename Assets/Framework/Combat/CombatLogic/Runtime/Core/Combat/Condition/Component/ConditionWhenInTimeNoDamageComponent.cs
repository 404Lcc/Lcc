using System;

namespace LccModel
{
    public class ConditionWhenInTimeNoDamageComponent : Component
    {
        public long time;
        private long noDamageTimer;

        public override void Awake<P1>(P1 p1)
        {
            base.Awake(p1);

            time = (long)(object)p1;

            Parent.GetParent<Combat>().ListenActionPoint(ActionPointType.PostReceiveDamage, WhenReceiveDamage);
        }

        public override void OnDestroy()
        {
            base.OnDestroy();

            Timer.Instance.RemoveTimer(noDamageTimer);

            Parent.GetParent<Combat>().UnListenActionPoint(ActionPointType.PostReceiveDamage, WhenReceiveDamage);
        }

        public void StartListen(Action whenNoDamageInTimeCallback)
        {
            noDamageTimer = Timer.Instance.NewOnceTimer(time, whenNoDamageInTimeCallback);
        }

        private void WhenReceiveDamage(Entity combatAction)
        {
            Timer.Instance.ResetTimer(noDamageTimer);
        }
    }
}