using System;

namespace LccModel
{
    public sealed class ConditionWhenInTimeNoDamageComponent : Component, IUpdate
    {
        private GameTimer NoDamageTimer { get; set; }
        public override bool DefaultEnable => false;

        public override void Awake<P1>(P1 p1)
        {
            base.Awake(p1);

            var time = (int)(object)p1;
            NoDamageTimer = new GameTimer(time);
            Parent.GetParent<CombatEntity>().ListenActionPoint(ActionPointType.PostReceiveDamage, WhenReceiveDamage);
        }

        public void StartListen(Action whenNoDamageInTimeCallback)
        {
            NoDamageTimer.OnFinish(whenNoDamageInTimeCallback);
            Enable = true;
        }

        public void Update()
        {
            if (NoDamageTimer.IsRunning)
            {
                NoDamageTimer.UpdateAsFinish(UnityEngine.Time.deltaTime);
            }
        }

        private void WhenReceiveDamage(Entity combatAction)
        {
            NoDamageTimer.Reset();
        }
    }
}