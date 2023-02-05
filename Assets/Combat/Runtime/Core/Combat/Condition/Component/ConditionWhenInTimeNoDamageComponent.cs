using System;

namespace LccModel
{
    public class ConditionWhenInTimeNoDamageComponent : Component, IUpdate
    {

        private GameTimer noDamageTimer;

        public override void Awake<P1>(P1 p1)
        {
            base.Awake(p1);

            float time = (float)(object)p1;
            noDamageTimer = new GameTimer(time);
            Parent.GetParent<CombatEntity>().ListenActionPoint(ActionPointType.PostReceiveDamage, WhenReceiveDamage);
        }

        public override void OnDestroy()
        {
            base.OnDestroy();

            Parent.GetParent<CombatEntity>().UnListenActionPoint(ActionPointType.PostReceiveDamage, WhenReceiveDamage);
        }

        public void StartListen(Action whenNoDamageInTimeCallback)
        {
            noDamageTimer.OnFinish(whenNoDamageInTimeCallback);
        }

        public void Update()
        {
            if (noDamageTimer.IsRunning)
            {
                noDamageTimer.UpdateAsFinish(UnityEngine.Time.deltaTime);
            }
        }

        private void WhenReceiveDamage(Entity combatAction)
        {
            noDamageTimer.Reset();
        }
    }
}